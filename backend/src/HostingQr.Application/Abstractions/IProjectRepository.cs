using HostingQr.Domain.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default);

    Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, CancellationToken cancellationToken = default);

    Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, CancellationToken cancellationToken = default);
}
