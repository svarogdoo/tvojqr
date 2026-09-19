using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Invitations;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Invitations;

public sealed class ProjectInvitationRepository : IProjectInvitationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public ProjectInvitationRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<ProjectInvitationRecord> CreateAsync(Guid projectId, Guid inviterUserId, string normalizedEmail, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into project_invitations (id, project_id, inviter_user_id, invited_email_normalized, token_hash, expires_at)
            values (@Id, @ProjectId, @InviterUserId, @NormalizedEmail, @TokenHash, @ExpiresAt)
            returning id, project_id as ProjectId, inviter_user_id as InviterUserId, invited_email_normalized as InvitedEmailNormalized,
                token_hash as TokenHash, expires_at as ExpiresAt, accepted_at as AcceptedAt, revoked_at as RevokedAt,
                created_at as CreatedAt, (select name from projects where id = @ProjectId) as ProjectName;
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<ProjectInvitationRecord>(new CommandDefinition(sql,
            new { Id = Guid.NewGuid(), ProjectId = projectId, InviterUserId = inviterUserId, NormalizedEmail = normalizedEmail, TokenHash = tokenHash, ExpiresAt = expiresAt }, cancellationToken: cancellationToken));
    }

    public async Task<ProjectInvitationRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select i.id, i.project_id as ProjectId, i.inviter_user_id as InviterUserId, i.invited_email_normalized as InvitedEmailNormalized,
                i.token_hash as TokenHash, i.expires_at as ExpiresAt, i.accepted_at as AcceptedAt, i.revoked_at as RevokedAt,
                i.created_at as CreatedAt, p.name as ProjectName
            from project_invitations i inner join projects p on p.id = i.project_id where i.token_hash = @TokenHash;
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProjectInvitationRecord>(new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));
    }

    public async Task<bool> RevokeAsync(Guid invitationId, Guid inviterUserId, CancellationToken cancellationToken = default)
    {
        const string sql = "update project_invitations set revoked_at = now() where id = @InvitationId and inviter_user_id = @InviterUserId and accepted_at is null and revoked_at is null;";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(new CommandDefinition(sql, new { InvitationId = invitationId, InviterUserId = inviterUserId }, cancellationToken: cancellationToken)) > 0;
    }

    public async Task<InvitationAcceptanceResult> AcceptAsync(string tokenHash, Guid acceptingUserId, string normalizedEmail, string entitlementTier, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        const string selectSql = "select id, project_id as ProjectId, inviter_user_id as InviterUserId, invited_email_normalized as InvitedEmailNormalized, expires_at as ExpiresAt, accepted_at as AcceptedAt, revoked_at as RevokedAt from project_invitations where token_hash = @TokenHash for update;";
        AcceptanceRow? invitation = await connection.QuerySingleOrDefaultAsync<AcceptanceRow>(new CommandDefinition(selectSql, new { TokenHash = tokenHash }, transaction, cancellationToken: cancellationToken));
        if (invitation is null)
        {
            transaction.Rollback();
            return InvitationAcceptanceResult.NotFound;
        }
        if (invitation.AcceptedAt is not null || invitation.RevokedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            transaction.Rollback();
            return InvitationAcceptanceResult.ExpiredOrUsed;
        }
        if (!string.Equals(invitation.InvitedEmailNormalized, normalizedEmail, StringComparison.Ordinal))
        {
            transaction.Rollback();
            return InvitationAcceptanceResult.EmailMismatch;
        }

        int transferred = await connection.ExecuteAsync(new CommandDefinition(
            "update projects set owner_user_id = @UserId, updated_at = now() where id = @ProjectId and owner_user_id = @InviterUserId;",
            new { UserId = acceptingUserId, invitation.ProjectId, invitation.InviterUserId }, transaction, cancellationToken: cancellationToken));
        if (transferred == 0)
        {
            transaction.Rollback();
            return InvitationAcceptanceResult.ExpiredOrUsed;
        }
        await connection.ExecuteAsync(new CommandDefinition("update project_invitations set accepted_at = now() where id = @Id;", new { invitation.Id }, transaction, cancellationToken: cancellationToken));
        const string entitlementSql = """
            insert into user_entitlements (user_id, tier, is_active, granted_manually, starts_at, ends_at, updated_at)
            values (@UserId, @Tier, true, true, now(), null, now())
            on conflict (user_id) do nothing;
            """;
        await connection.ExecuteAsync(new CommandDefinition(entitlementSql, new { UserId = acceptingUserId, Tier = entitlementTier }, transaction, cancellationToken: cancellationToken));
        transaction.Commit();
        return InvitationAcceptanceResult.Accepted;
    }

    private sealed record AcceptanceRow(Guid Id, Guid ProjectId, Guid InviterUserId, string InvitedEmailNormalized, DateTimeOffset ExpiresAt, DateTimeOffset? AcceptedAt, DateTimeOffset? RevokedAt);
}
