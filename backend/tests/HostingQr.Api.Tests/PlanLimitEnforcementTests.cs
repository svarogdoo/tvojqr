using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
using HostingQr.Application.Auth;
using HostingQr.Application.Billing;
using HostingQr.Application.Projects;
using HostingQr.Application.Slugs;
using HostingQr.Domain.Projects;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Api.Tests;

public sealed class PlanLimitEnforcementTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CreateProjectAsync_BlocksStandardUserAtProjectLimit()
    {
        FakeProjectRepository projectRepository = new();
        projectRepository.Projects.Add(new ProjectWithSlug
        {
            Id = Guid.NewGuid(),
            OwnerUserId = UserId,
            Name = "Existing",
            Slug = "existing",
            Status = ProjectStatus.Active,
            BackgroundColor = "#f8f7f3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        ProjectService service = CreateService(projectRepository, new FakeLanguageRepository(), BillingTier.Standard);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateProjectAsync(new CreateProjectRequest("Next", "next", "#f8f7f3", "en", "English")));

        Assert.Equal("Your Standard plan includes up to 1 project.", exception.Message);
    }

    [Fact]
    public async Task AddLanguageAsync_BlocksStandardUserAtLanguageLimit()
    {
        FakeProjectRepository projectRepository = new();
        Guid projectId = Guid.NewGuid();
        projectRepository.Projects.Add(new ProjectWithSlug
        {
            Id = projectId,
            OwnerUserId = UserId,
            Name = "Existing",
            Slug = "existing",
            Status = ProjectStatus.Active,
            BackgroundColor = "#f8f7f3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        FakeLanguageRepository languageRepository = new();
        languageRepository.Languages.AddRange([
            CreateLanguage(projectId, "en", true, 0),
            CreateLanguage(projectId, "de", false, 1),
            CreateLanguage(projectId, "it", false, 2),
        ]);
        ProjectService service = CreateService(projectRepository, languageRepository, BillingTier.Standard);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddLanguageAsync(projectId, new CreateProjectLanguageRequest("fr", "French")));

        Assert.Equal("Your Standard plan includes up to 3 languages per project.", exception.Message);
    }

    [Fact]
    public async Task AddLanguageAsync_AllowsPlusUserWithinLanguageLimit()
    {
        FakeProjectRepository projectRepository = new();
        Guid projectId = Guid.NewGuid();
        projectRepository.Projects.Add(new ProjectWithSlug
        {
            Id = projectId,
            OwnerUserId = UserId,
            Name = "Existing",
            Slug = "existing",
            Status = ProjectStatus.Active,
            BackgroundColor = "#f8f7f3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        FakeLanguageRepository languageRepository = new();
        languageRepository.Languages.Add(CreateLanguage(projectId, "en", true, 0));
        ProjectService service = CreateService(projectRepository, languageRepository, BillingTier.Plus);

        ProjectDetailResponse? response = await service.AddLanguageAsync(projectId, new CreateProjectLanguageRequest("fr", "French"));

        Assert.NotNull(response);
        Assert.Contains(response.Languages, language => language.LanguageCode == "fr");
    }

    [Fact]
    public async Task GetPublicProjectAsync_IncrementsViewCountForActiveProject()
    {
        Guid projectId = Guid.NewGuid();
        FakeProjectRepository projectRepository = new()
        {
            PublicProject = new PublicProject
            {
                ProjectId = projectId,
                Name = "Menu",
                Slug = "menu",
                OwnerDisplayName = "Demo User",
                Status = ProjectStatus.Active,
                BackgroundColor = "#f8f7f3",
            }
        };
        FakeProjectViewRepository viewRepository = new();
        ProjectService service = CreateService(projectRepository, new FakeLanguageRepository(), BillingTier.Standard, viewRepository);

        PublicProjectResponse? response = await service.GetPublicProjectAsync("menu");

        Assert.NotNull(response);
        Assert.Equal(1, viewRepository.IncrementCount);
    }

    [Fact]
    public async Task GetPublicProjectAsync_DoesNotIncrementViewCountForDisabledProject()
    {
        Guid projectId = Guid.NewGuid();
        FakeProjectRepository projectRepository = new()
        {
            PublicProject = new PublicProject
            {
                ProjectId = projectId,
                Name = "Menu",
                Slug = "menu",
                OwnerDisplayName = "Demo User",
                Status = ProjectStatus.Disabled,
                BackgroundColor = "#f8f7f3",
            }
        };
        FakeProjectViewRepository viewRepository = new();
        ProjectService service = CreateService(projectRepository, new FakeLanguageRepository(), BillingTier.Standard, viewRepository);

        PublicProjectResponse? response = await service.GetPublicProjectAsync("menu");

        Assert.NotNull(response);
        Assert.Equal(0, viewRepository.IncrementCount);
    }

    private static ProjectService CreateService(FakeProjectRepository projectRepository, FakeLanguageRepository languageRepository, string tier, FakeProjectViewRepository? viewRepository = null) => new(
        new FakeCurrentUserContext(),
        new FakeUserRepository(),
        projectRepository,
        viewRepository ?? new FakeProjectViewRepository(),
        new FakeSlugService(),
        new FakeEntitlementService(tier),
        new FakeAssetRepository(),
        new FakeAssetService(),
        languageRepository);

    private static ProjectLanguageVariant CreateLanguage(Guid projectId, string languageCode, bool isDefault, int sortOrder) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        LanguageCode = languageCode,
        DisplayName = languageCode.ToUpperInvariant(),
        IsDefault = isDefault,
        SortOrder = sortOrder,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid GetCurrentUserId() => UserId;

        public CurrentUser GetCurrentUser() => new(UserId, "demo@hostingqr.local", "Demo User");
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<AuthUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<AuthUserResponse?>(new AuthUserResponse(id, "demo@hostingqr.local", "Demo User"));

        public Task<AuthUserResponse> UpsertAsync(Guid id, string email, string displayName, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AuthUserResponse(id, email, displayName));
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        public List<ProjectWithSlug> Projects { get; } = [];

        public PublicProject? PublicProject { get; init; }

        public Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProjectWithSlug>>(Projects.Where(project => project.OwnerUserId == ownerUserId).ToArray());

        public Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Projects.SingleOrDefault(project => project.OwnerUserId == ownerUserId && project.Id == projectId));

        public Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(PublicProject?.Slug == slug ? PublicProject : null);

        public Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, string backgroundColor, CancellationToken cancellationToken = default)
        {
            ProjectWithSlug project = new()
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                Name = name,
                Slug = slug,
                Status = ProjectStatus.Active,
                BackgroundColor = backgroundColor,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            Projects.Add(project);
            return Task.FromResult(project);
        }

        public Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, string backgroundColor, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectWithSlug?>(null);

        public Task<ProjectWithSlug?> UpdateStatusAsync(Guid ownerUserId, Guid projectId, string status, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectWithSlug?>(null);

        public Task<bool> DeleteAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class FakeLanguageRepository : IProjectLanguageVariantRepository
    {
        public List<ProjectLanguageVariant> Languages { get; } = [];

        public Task<IReadOnlyList<ProjectLanguageVariant>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProjectLanguageVariant>>(Languages.Where(language => language.ProjectId == projectId).OrderBy(language => language.SortOrder).ToArray());

        public Task<ProjectLanguageVariant> CreateAsync(Guid projectId, string languageCode, string displayName, bool isDefault, int sortOrder, CancellationToken cancellationToken = default)
        {
            ProjectLanguageVariant language = CreateLanguage(projectId, languageCode, isDefault, sortOrder);
            language = language with { DisplayName = displayName };
            Languages.Add(language);
            return Task.FromResult(language);
        }

        public Task<ProjectLanguageVariant> UpdateAsync(Guid projectId, string currentLanguageCode, string languageCode, string displayName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<ProjectLanguageVariant> UpdateDefaultAsync(Guid projectId, string languageCode, string displayName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> DeleteAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class FakeProjectViewRepository : IProjectViewRepository
    {
        public int IncrementCount { get; private set; }

        public Task IncrementAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            IncrementCount++;
            return Task.CompletedTask;
        }

        public Task<ProjectViewStats?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectViewStats?>(new ProjectViewStats(projectId, 0, null));

        public Task<IReadOnlyDictionary<Guid, ProjectViewStats>> GetByProjectIdsAsync(IReadOnlyList<Guid> projectIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<Guid, ProjectViewStats>>(projectIds.ToDictionary(projectId => projectId, projectId => new ProjectViewStats(projectId, 0, null)));
    }

    private sealed class FakeSlugService : ISlugService
    {
        public string NormalizeOrThrow(string slug) => slug.Trim().ToLowerInvariant();

        public Task<SlugAvailabilityResponse> CheckAvailabilityAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SlugAvailabilityResponse(slug, true));

        public Task<bool> ExistsForOtherProjectAsync(string slug, Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<GeneratedSlugResponse> GenerateUniqueSlugAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new GeneratedSlugResponse("generated"));
    }

    private sealed class FakeEntitlementService : IEntitlementService
    {
        private readonly string _tier;

        public FakeEntitlementService(string tier)
        {
            _tier = tier;
        }

        public Task<EntitlementResponse> GetCurrentEntitlementAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new EntitlementResponse(_tier, true, false, null));

        public Task<PlanLimits> GetCurrentPlanLimitsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlanLimitCatalog.ForTier(_tier));

        public Task<bool> CurrentUserHasToolAccessAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class FakeAssetRepository : IAssetRepository
    {
        public Task<IReadOnlyList<Domain.Assets.Asset>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Domain.Assets.Asset>>([]);

        public Task<Domain.Assets.Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Domain.Assets.Asset?>(null);

        public Task<IReadOnlyList<Domain.Assets.Asset>> CreateAsync(Guid projectId, string languageCode, IReadOnlyList<CreateAssetRecord> assets, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Domain.Assets.Asset>>([]);

        public Task<bool> DeleteAsync(Guid assetId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task UpdateSortOrderAsync(Guid projectId, IReadOnlyList<Guid> assetIds, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeAssetService : IAssetService
    {
        public Task<IReadOnlyList<AssetResponse>> UploadImagesAsync(Guid projectId, string languageCode, IFormFileCollection files, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AssetResponse>>([]);

        public Task<bool> DeleteImageAsync(Guid projectId, Guid assetId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<AssetResponse>?> ReorderImagesAsync(Guid projectId, IReadOnlyList<Guid> assetIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AssetResponse>?>([]);

        public IReadOnlyList<AssetResponse> MapAssets(IReadOnlyList<Domain.Assets.Asset> assets) => [];
    }
}
