using HostingQr.Application.Billing;

namespace HostingQr.Application.Abstractions;

public interface IEntitlementService
{
    Task<EntitlementResponse> GetCurrentEntitlementAsync(CancellationToken cancellationToken = default);

    Task<PlanLimits> GetCurrentPlanLimitsAsync(CancellationToken cancellationToken = default);

    Task<bool> CurrentUserHasToolAccessAsync(CancellationToken cancellationToken = default);
}
