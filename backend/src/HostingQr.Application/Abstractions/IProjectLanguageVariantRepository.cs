using HostingQr.Domain.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectLanguageVariantRepository
{
    Task<IReadOnlyList<ProjectLanguageVariant>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectLanguageVariant> CreateAsync(Guid projectId, string languageCode, string displayName, bool isDefault, int sortOrder, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default);
}
