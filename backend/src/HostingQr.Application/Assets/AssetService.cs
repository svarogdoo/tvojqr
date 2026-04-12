using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Application.Assets;

public sealed class AssetService : IAssetService
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    ];

    private readonly ICurrentUserContext _currentUserContext;
    private readonly IProjectRepository _projectRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetStorageService _assetStorageService;

    public AssetService(
        ICurrentUserContext currentUserContext,
        IProjectRepository projectRepository,
        IAssetRepository assetRepository,
        IAssetStorageService assetStorageService)
    {
        _currentUserContext = currentUserContext;
        _projectRepository = projectRepository;
        _assetRepository = assetRepository;
        _assetStorageService = assetStorageService;
    }

    public async Task<IReadOnlyList<AssetResponse>> UploadImagesAsync(Guid projectId, IFormFileCollection files, CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("At least one image is required.");
        }

        Guid userId = _currentUserContext.GetCurrentUserId();
        var project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidOperationException("Project was not found.");
        }

        List<CreateAssetRecord> records = [];
        int startingOrder = (await _assetRepository.ListByProjectAsync(projectId, cancellationToken)).Count;

        for (int i = 0; i < files.Count; i++)
        {
            IFormFile file = files[i];
            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                throw new ArgumentException($"Unsupported file type '{file.ContentType}'.");
            }

            await using Stream stream = file.OpenReadStream();
            StoredAssetFile stored = await _assetStorageService.SaveImageAsync(projectId, stream, file.FileName, file.ContentType, cancellationToken);
            records.Add(new CreateAssetRecord(file.FileName, stored.StoredFileName, stored.ContentType, stored.SizeBytes, startingOrder + i));
        }

        var saved = await _assetRepository.CreateAsync(projectId, "default", records, cancellationToken);
        return MapAssets(saved);
    }

    public async Task<bool> DeleteImageAsync(Guid projectId, Guid assetId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        var project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            return false;
        }

        var asset = await _assetRepository.GetByIdAsync(assetId, cancellationToken);
        if (asset is null || asset.ProjectId != projectId)
        {
            return false;
        }

        await _assetStorageService.DeleteAsync(asset.StoredFileName, cancellationToken);
        return await _assetRepository.DeleteAsync(assetId, cancellationToken);
    }

    public IReadOnlyList<AssetResponse> MapAssets(IReadOnlyList<Domain.Assets.Asset> assets)
    {
        return assets
            .OrderBy(asset => asset.SortOrder)
            .Select(asset => new AssetResponse(
                asset.Id,
                asset.OriginalFileName,
                asset.ContentType,
                asset.SizeBytes,
                _assetStorageService.GetPublicUrl(asset.StoredFileName),
                asset.LanguageCode,
                asset.SortOrder,
                asset.CreatedAt))
            .ToArray();
    }
}
