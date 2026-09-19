namespace HostingQr.Application.Invitations;

public sealed record CreateProjectInvitationRequest(string Email, int ExpiresInDays = 7);

public sealed record ProjectInvitationResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);

public sealed record InvitationDetailsResponse(
    string ProjectName,
    DateTimeOffset ExpiresAt,
    bool IsAvailable);

public sealed record ProjectInvitationRecord(
    Guid Id,
    Guid ProjectId,
    Guid InviterUserId,
    string InvitedEmailNormalized,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt,
    string ProjectName);

public enum InvitationAcceptanceResult
{
    Accepted,
    NotFound,
    ExpiredOrUsed,
    EmailMismatch,
}
