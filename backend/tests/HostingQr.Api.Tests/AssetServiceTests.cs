using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
using HostingQr.Domain.Assets;
using HostingQr.Domain.Projects;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Api.Tests;

public sealed class AssetServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();

    [Fact]
    public async Task UploadDigitalMenuCoverAsync_ReplacesExistingCoverWithoutTouchingMenuAssets()
    {
        var menuAsset = CreateAsset(AssetPurpose.MenuContent, "menu.webp", "en");
        var oldCover = CreateAsset(AssetPurpose.DigitalMenuCover, "old-cover.webp", "und");
        var repository = new FakeAssetRepository([menuAsset, oldCover]);
        var storage = new FakeAssetStorageService("new-cover.webp");
        var service = CreateService(ProjectMenuType.Digital, repository, storage);

        AssetResponse result = await service.UploadDigitalMenuCoverAsync(_projectId, CreateFormFile("cover.jpg", "image/jpeg"));

        Assert.Equal("/uploads/new-cover.webp", result.Url);
        Assert.Contains(repository.Assets, asset => asset.Id == menuAsset.Id);
        Assert.DoesNotContain(repository.Assets, asset => asset.Id == oldCover.Id);
        Assert.Single(repository.Assets, asset => asset.Purpose == AssetPurpose.DigitalMenuCover);
        Assert.Contains("old-cover.webp", storage.DeletedFiles);
    }

    [Fact]
    public async Task UploadDigitalMenuCoverAsync_RejectsImageMenuProjects()
    {
        var storage = new FakeAssetStorageService("cover.webp");
        var service = CreateService(ProjectMenuType.Image, new FakeAssetRepository([]), storage);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadDigitalMenuCoverAsync(_projectId, CreateFormFile("cover.jpg", "image/jpeg")));

        Assert.Empty(storage.SavedFiles);
    }

    [Fact]
    public async Task UploadDigitalMenuCoverAsync_RejectsProjectsOutsideCurrentOwner()
    {
        var storage = new FakeAssetStorageService("cover.webp");
        var service = new AssetService(
            new FakeCurrentUserContext(_userId),
            new FakeProjectRepository(new ProjectWithSlug { Id = _projectId, OwnerUserId = Guid.NewGuid(), MenuType = ProjectMenuType.Digital }),
            new FakeAssetRepository([]),
            storage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadDigitalMenuCoverAsync(_projectId, CreateFormFile("cover.jpg", "image/jpeg")));

        Assert.Empty(storage.SavedFiles);
    }

    [Fact]
    public async Task DeleteDigitalMenuCoverAsync_RemovesOnlyCoverAssets()
    {
        var menuAsset = CreateAsset(AssetPurpose.MenuContent, "menu.webp", "en");
        var cover = CreateAsset(AssetPurpose.DigitalMenuCover, "cover.webp", "und");
        var repository = new FakeAssetRepository([menuAsset, cover]);
        var storage = new FakeAssetStorageService("unused.webp");
        var service = CreateService(ProjectMenuType.Digital, repository, storage);

        bool deleted = await service.DeleteDigitalMenuCoverAsync(_projectId);

        Assert.True(deleted);
        Assert.Single(repository.Assets);
        Assert.Equal(menuAsset.Id, repository.Assets[0].Id);
        Assert.Contains("cover.webp", storage.DeletedFiles);
    }

    private AssetService CreateService(string menuType, FakeAssetRepository repository, FakeAssetStorageService storage) => new(
        new FakeCurrentUserContext(_userId),
        new FakeProjectRepository(new ProjectWithSlug { Id = _projectId, OwnerUserId = _userId, MenuType = menuType }),
        repository,
        storage);

    private Asset CreateAsset(string purpose, string storedFileName, string languageCode) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = _projectId,
        Purpose = purpose,
        LanguageCode = languageCode,
        OriginalFileName = storedFileName,
        StoredFileName = storedFileName,
        ContentType = "image/webp",
        SizeBytes = 10,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static IFormFile CreateFormFile(string fileName, string contentType) =>
        new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };

    private sealed class FakeCurrentUserContext(Guid userId) : ICurrentUserContext
    {
        public Guid GetCurrentUserId() => userId;

        public CurrentUser GetCurrentUser() => new(userId, "owner@example.com", "Owner");
    }

    private sealed class FakeProjectRepository(ProjectWithSlug project) : IProjectRepository
    {
        public Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectWithSlug?>(project.OwnerUserId == ownerUserId && project.Id == projectId ? project : null);

        public Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, string backgroundColor, string menuType, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, string backgroundColor, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> UpdateStatusAsync(Guid ownerUserId, Guid projectId, string status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeAssetRepository(IEnumerable<Asset> initialAssets) : IAssetRepository
    {
        public List<Asset> Assets { get; } = [.. initialAssets];

        public Task<IReadOnlyList<Asset>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Asset>>([.. Assets.Where(asset => asset.ProjectId == projectId)]);

        public Task<IReadOnlyList<Asset>> CreateAsync(Guid projectId, string languageCode, IReadOnlyList<CreateAssetRecord> assets, CancellationToken cancellationToken = default)
        {
            Assets.AddRange(assets.Select(asset => new Asset
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                LanguageCode = languageCode,
                Purpose = asset.Purpose,
                OriginalFileName = asset.OriginalFileName,
                StoredFileName = asset.StoredFileName,
                ContentType = asset.ContentType,
                SizeBytes = asset.SizeBytes,
                SortOrder = asset.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow,
            }));
            return Task.FromResult<IReadOnlyList<Asset>>([.. Assets]);
        }

        public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Assets.SingleOrDefault(asset => asset.Id == assetId));

        public Task<bool> DeleteAsync(Guid assetId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Assets.RemoveAll(asset => asset.Id == assetId) > 0);

        public Task UpdateSortOrderAsync(Guid projectId, IReadOnlyList<Guid> assetIds, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeAssetStorageService(string nextStoredFileName) : IAssetStorageService
    {
        public List<string> SavedFiles { get; } = [];
        public List<string> DeletedFiles { get; } = [];

        public Task<StoredAssetFile> SaveImageAsync(Guid projectId, Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            SavedFiles.Add(nextStoredFileName);
            return Task.FromResult(new StoredAssetFile(nextStoredFileName, "image/webp", 3));
        }

        public string GetPublicUrl(string storedFileName) => $"/uploads/{storedFileName}";

        public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
        {
            DeletedFiles.Add(storedFileName);
            return Task.CompletedTask;
        }
    }
}
