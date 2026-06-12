using HostingQr.Application.Projects;

namespace HostingQr.Application.Abstractions;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> UpdateProjectStatusAsync(Guid projectId, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> AddLanguageAsync(Guid projectId, CreateProjectLanguageRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> UpdateLanguageAsync(Guid projectId, string languageCode, UpdateProjectLanguageRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDetailResponse?> DeleteLanguageAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default);

    Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<PublicProjectResponse?> GetPublicProjectAsync(string slug, CancellationToken cancellationToken = default);
}
