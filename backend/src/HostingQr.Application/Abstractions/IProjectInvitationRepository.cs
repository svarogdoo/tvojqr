using HostingQr.Application.Invitations;

namespace HostingQr.Application.Abstractions;

public interface IProjectInvitationRepository
{
    Task<ProjectInvitationRecord> CreateAsync(Guid projectId, Guid inviterUserId, string normalizedEmail, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
    Task<ProjectInvitationRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(Guid invitationId, Guid inviterUserId, CancellationToken cancellationToken = default);
    Task<InvitationAcceptanceResult> AcceptAsync(string tokenHash, Guid acceptingUserId, string normalizedEmail, string entitlementTier, CancellationToken cancellationToken = default);
}
