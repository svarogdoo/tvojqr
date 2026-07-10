using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Billing;

public sealed class EntitlementRepository : IEntitlementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EntitlementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EntitlementResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                tier as Tier,
                is_active as IsActive,
                granted_manually as GrantedManually,
                ends_at as EndsAt
            from user_entitlements
            where user_id = @UserId
                and is_active = true
                and (ends_at is null or ends_at > now())
            order by updated_at desc
            limit 1;
            """;

        using var connection = _connectionFactory.CreateConnection();
        EntitlementRow? entitlement = await connection.QuerySingleOrDefaultAsync<EntitlementRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return entitlement is null
            ? new EntitlementResponse(BillingTier.None, false, false, null)
            : new EntitlementResponse(
                entitlement.Tier,
                entitlement.IsActive,
                entitlement.GrantedManually,
                entitlement.EndsAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(entitlement.EndsAt.Value, DateTimeKind.Utc)));
    }

    public async Task UpsertAsync(Guid userId, string tier, bool isActive, DateTimeOffset? endsAt, bool grantedManually = false, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into user_entitlements (user_id, tier, is_active, granted_manually, ends_at, updated_at)
            values (@UserId, @Tier, @IsActive, @GrantedManually, @EndsAt, now())
            on conflict (user_id) do update set
                tier = excluded.tier,
                is_active = excluded.is_active,
                granted_manually = excluded.granted_manually,
                ends_at = excluded.ends_at,
                updated_at = now();
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = userId,
            Tier = tier,
            IsActive = isActive,
            GrantedManually = grantedManually,
            EndsAt = endsAt?.UtcDateTime,
        }, cancellationToken: cancellationToken));
    }

    private sealed class EntitlementRow
    {
        public string Tier { get; init; } = BillingTier.None;

        public bool IsActive { get; init; }

        public bool GrantedManually { get; init; }

        public DateTime? EndsAt { get; init; }
    }
}
