using HostingQr.Application.Abstractions;

namespace HostingQr.Application.Projects;

public sealed class ProjectService : IProjectService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IProjectRepository _projectRepository;
    private readonly ISlugService _slugService;

    public ProjectService(ICurrentUserContext currentUserContext, IProjectRepository projectRepository, ISlugService slugService)
    {
        _currentUserContext = currentUserContext;
        _projectRepository = projectRepository;
        _slugService = slugService;
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        IReadOnlyList<Domain.Projects.ProjectWithSlug> projects = await _projectRepository.ListByOwnerAsync(userId, cancellationToken);

        return projects
            .Select(project => new ProjectListItem(project.Id, project.Name, project.Slug, project.UpdatedAt))
            .ToArray();
    }

    public async Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);

        return project is null
            ? null
            : new ProjectDetailResponse(project.Id, project.Name, project.Slug, project.CreatedAt, project.UpdatedAt);
    }

    public async Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Project name is required.", nameof(request));
        }

        Guid userId = _currentUserContext.GetCurrentUserId();
        string normalizedSlug = _slugService.NormalizeOrThrow(request.Slug);
        Slugs.SlugAvailabilityResponse availability = await _slugService.CheckAvailabilityAsync(normalizedSlug, cancellationToken);
        if (!availability.IsAvailable)
        {
            throw new InvalidOperationException("Slug is already in use.");
        }

        Domain.Projects.ProjectWithSlug project = await _projectRepository.CreateAsync(userId, request.Name.Trim(), normalizedSlug, cancellationToken);
        return new ProjectDetailResponse(project.Id, project.Name, project.Slug, project.CreatedAt, project.UpdatedAt);
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
            : new ProjectDetailResponse(updatedProject.Id, updatedProject.Name, updatedProject.Slug, updatedProject.CreatedAt, updatedProject.UpdatedAt);
    }

    public async Task<PublicProjectResponse?> GetPublicProjectAsync(string slug, CancellationToken cancellationToken = default)
    {
        string normalizedSlug = _slugService.NormalizeOrThrow(slug);
        Domain.Projects.PublicProject? project = await _projectRepository.GetPublicBySlugAsync(normalizedSlug, cancellationToken);

        return project is null
            ? null
            : new PublicProjectResponse(project.ProjectId, project.Name, project.Slug, project.OwnerDisplayName);
    }
}
