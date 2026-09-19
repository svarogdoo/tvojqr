using HostingQr.Domain.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectAccessService
{
    Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default);
    Task<ProjectWithSlug?> GetAccessibleProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
