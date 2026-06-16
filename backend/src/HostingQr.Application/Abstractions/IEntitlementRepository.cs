using HostingQr.Application.Billing;

namespace HostingQr.Application.Abstractions;

public interface IEntitlementRepository
{
    Task<EntitlementResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
