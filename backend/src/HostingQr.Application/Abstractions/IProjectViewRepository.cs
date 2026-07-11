using HostingQr.Application.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectViewRepository
{
    Task IncrementAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectViewStats?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ProjectViewStats>> GetByProjectIdsAsync(IReadOnlyList<Guid> projectIds, CancellationToken cancellationToken = default);
}
