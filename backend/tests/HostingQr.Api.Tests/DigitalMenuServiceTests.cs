using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Application.Menus;
using HostingQr.Domain.Projects;

namespace HostingQr.Api.Tests;

public sealed class DigitalMenuServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task GetPublicAsync_FiltersSectionsOutsideCurrentServingWindow()
    {
        DigitalMenuCategoryResponse visible = Category("Breakfast", new DigitalMenuSchedule(1, "09:00", "11:00"));
        DigitalMenuCategoryResponse hidden = Category("Lunch", new DigitalMenuSchedule(1, "11:00", "15:00"));
        FakeMenuRepository repository = new(new DigitalMenuResponse("UTC", [visible, hidden]));
        DigitalMenuService service = CreateService(repository, new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero));

        DigitalMenuResponse result = await service.GetPublicAsync(ProjectId, "UTC");

        Assert.Single(result.Categories);
        Assert.Equal("Breakfast", result.Categories[0].Translations[0].Name);
    }

    [Fact]
    public async Task GetPublicAsync_SupportsServingWindowsAcrossMidnight()
    {
        DigitalMenuCategoryResponse lateMenu = Category("Late menu", new DigitalMenuSchedule(1, "22:00", "02:00"));
        FakeMenuRepository repository = new(new DigitalMenuResponse("UTC", [lateMenu]));
        DigitalMenuService service = CreateService(repository, new DateTimeOffset(2026, 9, 8, 1, 0, 0, TimeSpan.Zero));

        DigitalMenuResponse result = await service.GetPublicAsync(ProjectId, "UTC");

        Assert.Single(result.Categories);
    }

    [Fact]
    public async Task SaveAsync_RejectsEqualScheduleTimes()
    {
        FakeMenuRepository repository = new(new DigitalMenuResponse("UTC", []));
        DigitalMenuService service = CreateService(repository, new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero));
        SaveDigitalMenuRequest request = new("UTC", [new DigitalMenuCategoryRequest(
            Guid.NewGuid(),
            [new DigitalMenuCategoryTranslation("en", "Breakfast")],
            [new DigitalMenuSchedule(1, "09:00", "09:00")],
            [])]);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(ProjectId, request));

        Assert.Contains("different", exception.Message);
        Assert.False(repository.SaveCalled);
    }

    private static DigitalMenuCategoryResponse Category(string name, params DigitalMenuSchedule[] schedules) =>
        new(Guid.NewGuid(), 0, [new DigitalMenuCategoryTranslation("en", name)], schedules, []);

    private static DigitalMenuService CreateService(FakeMenuRepository repository, DateTimeOffset now) => new(
        new FakeCurrentUserContext(),
        new FakeProjectRepository(),
        new FakeLanguageRepository(),
        repository,
        new FakeEntitlementService(),
        new FixedTimeProvider(now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid GetCurrentUserId() => UserId;
        public CurrentUser GetCurrentUser() => new(UserId, "owner@example.com", "Owner");
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private static readonly ProjectWithSlug Project = new()
        {
            Id = ProjectId,
            OwnerUserId = UserId,
            Name = "Menu",
            Slug = "menu",
            Status = ProjectStatus.Active,
            MenuType = ProjectMenuType.Digital,
            TimeZone = "UTC",
            BackgroundColor = "#f8f7f3",
        };

        public Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult<ProjectWithSlug?>(ownerUserId == UserId && projectId == ProjectId ? Project : null);
        public Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProjectWithSlug>>([Project]);
        public Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, string backgroundColor, string menuType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, string backgroundColor, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProjectWithSlug?> UpdateStatusAsync(Guid ownerUserId, Guid projectId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeLanguageRepository : IProjectLanguageVariantRepository
    {
        public Task<IReadOnlyList<ProjectLanguageVariant>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProjectLanguageVariant>>([new ProjectLanguageVariant { Id = Guid.NewGuid(), ProjectId = projectId, LanguageCode = "en", DisplayName = "English", IsDefault = true }]);
        public Task<ProjectLanguageVariant> CreateAsync(Guid projectId, string languageCode, string displayName, bool isDefault, int sortOrder, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProjectLanguageVariant> UpdateAsync(Guid projectId, string currentLanguageCode, string languageCode, string displayName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProjectLanguageVariant> UpdateDefaultAsync(Guid projectId, string languageCode, string displayName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeMenuRepository(DigitalMenuResponse menu) : IDigitalMenuRepository
    {
        public bool SaveCalled { get; private set; }
        public Task<DigitalMenuResponse> GetAsync(Guid projectId, string timeZone, CancellationToken cancellationToken = default) => Task.FromResult(menu);
        public Task SaveAsync(Guid projectId, SaveDigitalMenuRequest request, CancellationToken cancellationToken = default) { SaveCalled = true; return Task.CompletedTask; }
        public Task<bool> UpdateItemAvailabilityAsync(Guid projectId, Guid itemId, bool isOutOfStock, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class FakeEntitlementService : IEntitlementService
    {
        public Task<EntitlementResponse> GetCurrentEntitlementAsync(CancellationToken cancellationToken = default) => Task.FromResult(new EntitlementResponse(BillingTier.Plus, true, false, null));
        public Task<PlanLimits> GetCurrentPlanLimitsAsync(CancellationToken cancellationToken = default) => Task.FromResult(PlanLimitCatalog.ForTier(BillingTier.Plus));
        public Task<bool> CurrentUserHasToolAccessAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
