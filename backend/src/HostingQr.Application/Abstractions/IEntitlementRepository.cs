using HostingQr.Application.Billing;

namespace HostingQr.Application.Abstractions;

public interface IEntitlementRepository
{
    Task<EntitlementResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpsertAsync(Guid userId, string tier, bool isActive, DateTimeOffset? endsAt, bool grantedManually = false, CancellationToken cancellationToken = default);
}
