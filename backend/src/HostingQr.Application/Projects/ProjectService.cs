using HostingQr.Application.Abstractions;

namespace HostingQr.Application.Projects;

public sealed class ProjectService : IProjectService
{
    private const string DefaultBackgroundColor = "#f8f7f3";

    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectViewRepository _projectViewRepository;
    private readonly ISlugService _slugService;
    private readonly IEntitlementService _entitlementService;
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetService _assetService;
    private readonly IProjectLanguageVariantRepository _languageVariantRepository;

    public ProjectService(
        ICurrentUserContext currentUserContext,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IProjectViewRepository projectViewRepository,
        ISlugService slugService,
        IEntitlementService entitlementService,
        IAssetRepository assetRepository,
        IAssetService assetService,
        IProjectLanguageVariantRepository languageVariantRepository)
    {
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _projectViewRepository = projectViewRepository;
        _slugService = slugService;
        _entitlementService = entitlementService;
        _assetRepository = assetRepository;
        _assetService = assetService;
        _languageVariantRepository = languageVariantRepository;
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        IReadOnlyList<Domain.Projects.ProjectWithSlug> projects = await _projectRepository.ListByOwnerAsync(userId, cancellationToken);
        IReadOnlyDictionary<Guid, ProjectViewStats> viewStats = await _projectViewRepository.GetByProjectIdsAsync(projects.Select(project => project.Id).ToArray(), cancellationToken);

        return projects
            .Select(project =>
            {
                ProjectViewStats stats = GetStatsOrDefault(viewStats, project.Id);
                return new ProjectListItem(project.Id, project.Name, project.Slug, project.Status, project.UpdatedAt, stats.TotalViews, stats.LastViewedAt);
            })
            .ToArray();
    }

    public async Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);

        return project is null
            ? null
            : await MapProjectDetailAsync(project, cancellationToken);
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

        string backgroundColor = NormalizeBackgroundColor(request.BackgroundColor);
        Billing.PlanLimits limits = await _entitlementService.GetCurrentPlanLimitsAsync(cancellationToken);
        int currentProjectCount = (await _projectRepository.ListByOwnerAsync(userId, cancellationToken)).Count;
        if (currentProjectCount >= limits.MaxProjects)
        {
            throw new InvalidOperationException($"Your {FormatTierName(limits.Tier)} plan includes up to {FormatLimit(limits.MaxProjects)} project{Pluralize(limits.MaxProjects)}.");
        }

        Domain.Projects.ProjectWithSlug project = await _projectRepository.CreateAsync(userId, request.Name.Trim(), normalizedSlug, backgroundColor, cancellationToken);
        string defaultLanguageCode = NormalizeLanguageCode(request.DefaultLanguageCode);
        string defaultLanguageDisplayName = NormalizeLanguageDisplayName(request.DefaultLanguageDisplayName, defaultLanguageCode);
        await _languageVariantRepository.CreateAsync(project.Id, defaultLanguageCode, defaultLanguageDisplayName, true, 0, cancellationToken);
        return await MapProjectDetailAsync(project, cancellationToken);
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

        string backgroundColor = NormalizeBackgroundColor(request.BackgroundColor);
        Domain.Projects.ProjectWithSlug? updatedProject = await _projectRepository.UpdateAsync(userId, projectId, request.Name.Trim(), normalizedSlug, backgroundColor, cancellationToken);
        if (updatedProject is null)
        {
            return null;
        }

        return await MapProjectDetailAsync(updatedProject, cancellationToken);
    }

    public async Task<ProjectDetailResponse?> UpdateProjectStatusAsync(Guid projectId, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default)
    {
        string normalizedStatus = NormalizeStatus(request.Status);
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? updatedProject = await _projectRepository.UpdateStatusAsync(userId, projectId, normalizedStatus, cancellationToken);

        return updatedProject is null
            ? null
            : await MapProjectDetailAsync(updatedProject, cancellationToken);
    }

    public async Task<ProjectDetailResponse?> AddLanguageAsync(Guid projectId, CreateProjectLanguageRequest request, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        string languageCode = NormalizeLanguageCode(request.LanguageCode);
        string displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? languageCode.ToUpperInvariant() : request.DisplayName.Trim();
        var languages = await _languageVariantRepository.ListByProjectAsync(projectId, cancellationToken);
        Billing.PlanLimits limits = await _entitlementService.GetCurrentPlanLimitsAsync(cancellationToken);
        if (languages.Count >= limits.MaxLanguagesPerProject)
        {
            throw new InvalidOperationException($"Your {FormatTierName(limits.Tier)} plan includes up to {FormatLimit(limits.MaxLanguagesPerProject)} language{Pluralize(limits.MaxLanguagesPerProject)} per project.");
        }

        if (languages.Any(language => language.LanguageCode == languageCode))
        {
            throw new InvalidOperationException("Language is already added to this project.");
        }

        await _languageVariantRepository.CreateAsync(projectId, languageCode, displayName, false, languages.Count, cancellationToken);
        return await MapProjectDetailAsync(project, cancellationToken);
    }

    public async Task<ProjectDetailResponse?> UpdateLanguageAsync(Guid projectId, string languageCode, UpdateProjectLanguageRequest request, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        string currentLanguageCode = NormalizeLanguageCode(languageCode);
        string nextLanguageCode = NormalizeLanguageCode(request.LanguageCode);
        string displayName = NormalizeLanguageDisplayName(request.DisplayName, nextLanguageCode);
        var languages = await _languageVariantRepository.ListByProjectAsync(projectId, cancellationToken);
        var currentLanguage = languages.SingleOrDefault(language => language.LanguageCode == currentLanguageCode);
        if (currentLanguage is null)
        {
            return null;
        }

        if (currentLanguage.IsDefault)
        {
            await _languageVariantRepository.UpdateDefaultAsync(projectId, nextLanguageCode, displayName, cancellationToken);
        }
        else
        {
            await _languageVariantRepository.UpdateAsync(projectId, currentLanguageCode, nextLanguageCode, displayName, cancellationToken);
        }

        return await MapProjectDetailAsync(project, cancellationToken);
    }

    public async Task<ProjectDetailResponse?> DeleteLanguageAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        Domain.Projects.ProjectWithSlug? project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        string normalizedLanguageCode = NormalizeLanguageCode(languageCode);
        var languages = await _languageVariantRepository.ListByProjectAsync(projectId, cancellationToken);
        if (languages.SingleOrDefault(language => language.LanguageCode == normalizedLanguageCode) is not { } language)
        {
            return null;
        }

        if (language.IsDefault)
        {
            throw new ArgumentException("Default language cannot be removed.");
        }

        var assets = await _assetRepository.ListByProjectAsync(projectId, cancellationToken);
        foreach (var asset in assets.Where(asset => asset.LanguageCode == normalizedLanguageCode))
        {
            await _assetService.DeleteImageAsync(projectId, asset.Id, cancellationToken);
        }

        await _languageVariantRepository.DeleteAsync(projectId, normalizedLanguageCode, cancellationToken);
        return await MapProjectDetailAsync(project, cancellationToken);
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

        if (project is null)
        {
            return null;
        }

        if (project.Status == Domain.Projects.ProjectStatus.Active)
        {
            await _projectViewRepository.IncrementAsync(project.ProjectId, cancellationToken);
        }

        return new PublicProjectResponse(
            project.ProjectId,
            project.Name,
            project.Slug,
            project.OwnerDisplayName,
            project.Status,
            project.BackgroundColor,
            await GetLanguagesAsync(project.ProjectId, cancellationToken),
            project.Status == Domain.Projects.ProjectStatus.Active
                ? await GetAssetsAsync(project.ProjectId, cancellationToken)
                : []);
    }

    private async Task<ProjectDetailResponse> MapProjectDetailAsync(Domain.Projects.ProjectWithSlug project, CancellationToken cancellationToken)
    {
        ProjectViewStats stats = await GetStatsOrDefaultAsync(project.Id, cancellationToken);
        return new ProjectDetailResponse(
            project.Id,
            project.Name,
            project.Slug,
            project.Status,
            project.BackgroundColor,
            project.CreatedAt,
            project.UpdatedAt,
            stats.TotalViews,
            stats.LastViewedAt,
            await GetLanguagesAsync(project.Id, cancellationToken),
            await GetAssetsAsync(project.Id, cancellationToken));
    }

    private async Task<ProjectViewStats> GetStatsOrDefaultAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _projectViewRepository.GetByProjectIdAsync(projectId, cancellationToken) ?? new ProjectViewStats(projectId, 0, null);

    private static ProjectViewStats GetStatsOrDefault(IReadOnlyDictionary<Guid, ProjectViewStats> stats, Guid projectId) =>
        stats.TryGetValue(projectId, out ProjectViewStats? projectStats) ? projectStats : new ProjectViewStats(projectId, 0, null);

    private async Task<IReadOnlyList<Assets.AssetResponse>> GetAssetsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var assets = await _assetRepository.ListByProjectAsync(projectId, cancellationToken);
        return _assetService.MapAssets(assets);
    }

    private async Task<IReadOnlyList<ProjectLanguageVariantResponse>> GetLanguagesAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var languages = await _languageVariantRepository.ListByProjectAsync(projectId, cancellationToken);
        return languages
            .Select(language => new ProjectLanguageVariantResponse(language.Id, language.LanguageCode, language.DisplayName, language.IsDefault, language.SortOrder))
            .ToArray();
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

    private static string FormatTierName(string tier) => tier switch
    {
        Billing.BillingTier.Admin => "Admin",
        Billing.BillingTier.Free => "Free",
        Billing.BillingTier.Standard => "Standard",
        Billing.BillingTier.Plus => "Plus",
        _ => "current",
    };

    private static string FormatLimit(int limit) => limit == int.MaxValue ? "unlimited" : limit.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Pluralize(int count) => count == 1 ? string.Empty : "s";

    private static string NormalizeBackgroundColor(string? backgroundColor)
    {
        if (string.IsNullOrWhiteSpace(backgroundColor))
        {
            return DefaultBackgroundColor;
        }

        string normalized = backgroundColor.Trim().ToLowerInvariant();
        if (System.Text.RegularExpressions.Regex.IsMatch(normalized, "^#[0-9a-f]{6}$"))
        {
            return normalized;
        }

        throw new ArgumentException("Background color must be a valid hex color, for example #f8f7f3.", nameof(backgroundColor));
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        string normalized = languageCode.Trim().ToLowerInvariant();
        if (System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z]{2}$"))
        {
            return normalized;
        }

        throw new ArgumentException("Language code must be a two-letter code, for example en.", nameof(languageCode));
    }

    private static string NormalizeLanguageDisplayName(string? displayName, string languageCode)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? languageCode.ToUpperInvariant()
            : displayName.Trim();
    }

}
