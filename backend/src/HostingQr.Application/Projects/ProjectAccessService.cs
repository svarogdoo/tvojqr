using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Domain.Projects;

namespace HostingQr.Application.Projects;

public sealed class ProjectAccessService : IProjectAccessService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEntitlementService _entitlementService;
    private readonly IProjectRepository _projectRepository;

    public ProjectAccessService(
        ICurrentUserContext currentUserContext,
        IEntitlementService entitlementService,
        IProjectRepository projectRepository)
    {
        _currentUserContext = currentUserContext;
        _entitlementService = entitlementService;
        _projectRepository = projectRepository;
    }

    public async Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default)
    {
        EntitlementResponse entitlement = await _entitlementService.GetCurrentEntitlementAsync(cancellationToken);
        return entitlement.IsActive && entitlement.Tier == BillingTier.Admin;
    }

    public async Task<ProjectWithSlug?> GetAccessibleProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        ProjectWithSlug? owned = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (owned is not null)
        {
            return owned;
        }

        return await IsCurrentUserAdminAsync(cancellationToken)
            ? await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            : null;
    }
}
