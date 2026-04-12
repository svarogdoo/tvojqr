using HostingQr.Application.Abstractions;

namespace HostingQr.Application.Projects;

public sealed class ProjectService : IProjectService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ISlugService _slugService;
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetService _assetService;

    public ProjectService(
        ICurrentUserContext currentUserContext,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        ISlugService slugService,
        IAssetRepository assetRepository,
        IAssetService assetService)
    {
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _slugService = slugService;
        _assetRepository = assetRepository;
        _assetService = assetService;
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        IReadOnlyList<Domain.Projects.ProjectWithSlug> projects = await _projectRepository.ListByOwnerAsync(userId, cancellationToken);

        return projects
            .Select(project => new ProjectListItem(project.Id, project.Name, project.Slug, project.Status, project.UpdatedAt))
            .ToArray();
    }

    public async Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);

        return project is null
            ? null
            : new ProjectDetailResponse(project.Id, project.Name, project.Slug, project.Status, project.CreatedAt, project.UpdatedAt, await GetAssetsAsync(project.Id, cancellationToken));
    }

    public async Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserContext.GetCurrentUser();
        Guid userId = currentUser.Id;
        string normalizedSlug = _slugService.NormalizeOrThrow(request.Slug);
        Slugs.SlugAvailabilityResponse availability = await _slugService.CheckAvailabilityAsync(normalizedSlug, cancellationToken);
        if (!availability.IsAvailable)
        {
            throw new InvalidOperationException("Slug is already in use.");
        }

        await _userRepository.UpsertAsync(currentUser.Id, currentUser.Email, currentUser.DisplayName, cancellationToken);

        Domain.Projects.ProjectWithSlug project = await _projectRepository.CreateAsync(userId, request.Name.Trim(), normalizedSlug, cancellationToken);
        return new ProjectDetailResponse(project.Id, project.Name, project.Slug, project.Status, project.CreatedAt, project.UpdatedAt, []);
    }

    public async Task<ProjectDetailResponse?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Project name is required.", nameof(request));
        }

        Guid userId = _currentUserContext.GetCurrentUserId();
        string normalizedSlug = _slugService.NormalizeOrThrow(request.Slug);

        if (await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken) is not { } existingProject)
        {
            return null;
        }

        bool slugTakenElsewhere = await _slugService.ExistsForOtherProjectAsync(normalizedSlug, projectId, cancellationToken);
        if (slugTakenElsewhere)
        {
            throw new InvalidOperationException("Slug is already in use.");
        }

        Domain.Projects.ProjectWithSlug? updatedProject = await _projectRepository.UpdateAsync(userId, projectId, request.Name.Trim(), normalizedSlug, cancellationToken);
        return updatedProject is null
            ? null
            : new ProjectDetailResponse(updatedProject.Id, updatedProject.Name, updatedProject.Slug, updatedProject.Status, updatedProject.CreatedAt, updatedProject.UpdatedAt, await GetAssetsAsync(updatedProject.Id, cancellationToken));
    }

    public async Task<ProjectDetailResponse?> UpdateProjectStatusAsync(Guid projectId, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default)
    {
        string normalizedStatus = NormalizeStatus(request.Status);
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? updatedProject = await _projectRepository.UpdateStatusAsync(userId, projectId, normalizedStatus, cancellationToken);

        return updatedProject is null
            ? null
            : new ProjectDetailResponse(updatedProject.Id, updatedProject.Name, updatedProject.Slug, updatedProject.Status, updatedProject.CreatedAt, updatedProject.UpdatedAt, await GetAssetsAsync(updatedProject.Id, cancellationToken));
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        return await _projectRepository.DeleteAsync(userId, projectId, cancellationToken);
    }

    public async Task<PublicProjectResponse?> GetPublicProjectAsync(string slug, CancellationToken cancellationToken = default)
    {
        string normalizedSlug = _slugService.NormalizeOrThrow(slug);
        Domain.Projects.PublicProject? project = await _projectRepository.GetPublicBySlugAsync(normalizedSlug, cancellationToken);

        return project is null
            ? null
            : new PublicProjectResponse(
                project.ProjectId,
                project.Name,
                project.Slug,
                project.OwnerDisplayName,
                project.Status,
                project.Status == Domain.Projects.ProjectStatus.Active
                    ? await GetAssetsAsync(project.ProjectId, cancellationToken)
                    : []);
    }

    private async Task<IReadOnlyList<Assets.AssetResponse>> GetAssetsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var assets = await _assetRepository.ListByProjectAsync(projectId, cancellationToken);
        return _assetService.MapAssets(assets);
    }

    private static string NormalizeStatus(string status)
    {
        string normalized = status.Trim().ToLowerInvariant();
        return normalized switch
        {
            Domain.Projects.ProjectStatus.Active => Domain.Projects.ProjectStatus.Active,
            Domain.Projects.ProjectStatus.Disabled => Domain.Projects.ProjectStatus.Disabled,
            _ => throw new ArgumentException("Project status is invalid.", nameof(status)),
        };
    }
}
