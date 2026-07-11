using HostingQr.Application.Abstractions;

namespace HostingQr.Application.Billing;

public sealed class EntitlementService : IEntitlementService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEntitlementRepository _entitlementRepository;

    public EntitlementService(ICurrentUserContext currentUserContext, IEntitlementRepository entitlementRepository)
    {
        _currentUserContext = currentUserContext;
        _entitlementRepository = entitlementRepository;
    }

    public Task<EntitlementResponse> GetCurrentEntitlementAsync(CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        return _entitlementRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<PlanLimits> GetCurrentPlanLimitsAsync(CancellationToken cancellationToken = default)
    {
        EntitlementResponse entitlement = await GetCurrentEntitlementAsync(cancellationToken);
        return entitlement.HasToolAccess
            ? PlanLimitCatalog.ForTier(entitlement.Tier)
            : PlanLimitCatalog.ForTier(BillingTier.None);
    }

    public async Task<bool> CurrentUserHasToolAccessAsync(CancellationToken cancellationToken = default)
    {
        EntitlementResponse entitlement = await GetCurrentEntitlementAsync(cancellationToken);
        return entitlement.HasToolAccess;
    }
}
