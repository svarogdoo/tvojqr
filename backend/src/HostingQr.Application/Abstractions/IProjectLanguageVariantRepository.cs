using HostingQr.Domain.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectLanguageVariantRepository
{
    Task<IReadOnlyList<ProjectLanguageVariant>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectLanguageVariant> CreateAsync(Guid projectId, string languageCode, string displayName, bool isDefault, int sortOrder, CancellationToken cancellationToken = default);

    Task<ProjectLanguageVariant> UpdateAsync(Guid projectId, string currentLanguageCode, string languageCode, string displayName, CancellationToken cancellationToken = default);

    Task<ProjectLanguageVariant> UpdateDefaultAsync(Guid projectId, string languageCode, string displayName, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default);
}
