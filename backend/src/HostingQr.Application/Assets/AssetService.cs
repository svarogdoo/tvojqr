using HostingQr.Application.Abstractions;
using HostingQr.Domain.Assets;
using HostingQr.Domain.Projects;
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

    public async Task<IReadOnlyList<AssetResponse>> UploadImagesAsync(Guid projectId, string languageCode, IFormFileCollection files, CancellationToken cancellationToken = default)
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

        string normalizedLanguageCode = NormalizeLanguageCode(languageCode);
        List<CreateAssetRecord> records = [];
        int startingOrder = (await _assetRepository.ListByProjectAsync(projectId, cancellationToken))
            .Count(asset => asset.Purpose == AssetPurpose.MenuContent && asset.LanguageCode == normalizedLanguageCode);

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

        var saved = await _assetRepository.CreateAsync(projectId, normalizedLanguageCode, records, cancellationToken);
        return MapAssets(saved.Where(asset => asset.Purpose == AssetPurpose.MenuContent).ToArray());
    }

    public async Task<AssetResponse> UploadDigitalMenuCoverAsync(Guid projectId, IFormFile file, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        var project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidOperationException("Project was not found.");
        }

        if (project.MenuType != ProjectMenuType.Digital)
        {
            throw new ArgumentException("Cover images are available only for Digital Menu projects.");
        }

        ValidateImage(file);
        await using Stream stream = file.OpenReadStream();
        StoredAssetFile stored = await _assetStorageService.SaveImageAsync(projectId, stream, file.FileName, file.ContentType, cancellationToken);

        IReadOnlyList<Domain.Assets.Asset> savedAssets;
        try
        {
            savedAssets = await _assetRepository.CreateAsync(projectId, "und",
            [
                new CreateAssetRecord(file.FileName, stored.StoredFileName, stored.ContentType, stored.SizeBytes, 0, AssetPurpose.DigitalMenuCover),
            ], cancellationToken);
        }
        catch
        {
            await _assetStorageService.DeleteAsync(stored.StoredFileName, cancellationToken);
            throw;
        }

        var savedCover = savedAssets.Single(asset => asset.StoredFileName == stored.StoredFileName);
        foreach (var oldCover in savedAssets.Where(asset => asset.Purpose == AssetPurpose.DigitalMenuCover && asset.Id != savedCover.Id))
        {
            await _assetStorageService.DeleteAsync(oldCover.StoredFileName, cancellationToken);
            await _assetRepository.DeleteAsync(oldCover.Id, cancellationToken);
        }

        return MapAsset(savedCover);
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
        if (asset is null || asset.ProjectId != projectId || asset.Purpose != AssetPurpose.MenuContent)
        {
            return false;
        }

        await _assetStorageService.DeleteAsync(asset.StoredFileName, cancellationToken);
        return await _assetRepository.DeleteAsync(assetId, cancellationToken);
    }

    public async Task<bool> DeleteDigitalMenuCoverAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        var project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null || project.MenuType != ProjectMenuType.Digital)
        {
            return false;
        }

        var covers = (await _assetRepository.ListByProjectAsync(projectId, cancellationToken))
            .Where(asset => asset.Purpose == AssetPurpose.DigitalMenuCover)
            .ToArray();
        foreach (var cover in covers)
        {
            await _assetStorageService.DeleteAsync(cover.StoredFileName, cancellationToken);
            await _assetRepository.DeleteAsync(cover.Id, cancellationToken);
        }

        return covers.Length > 0;
    }

    public async Task<IReadOnlyList<AssetResponse>?> ReorderImagesAsync(Guid projectId, IReadOnlyList<Guid> assetIds, CancellationToken cancellationToken = default)
    {
        Guid userId = _currentUserContext.GetCurrentUserId();
        var project = await _projectRepository.GetByIdAsync(userId, projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        if (assetIds.Count != assetIds.Distinct().Count())
        {
            throw new ArgumentException("Asset order contains duplicate images.");
        }

        var existingAssets = (await _assetRepository.ListByProjectAsync(projectId, cancellationToken))
            .Where(asset => asset.Purpose == AssetPurpose.MenuContent)
            .ToArray();
        var orderedAssets = assetIds
            .Select(assetId => existingAssets.SingleOrDefault(asset => asset.Id == assetId))
            .ToArray();
        if (orderedAssets.Any(asset => asset is null))
        {
            throw new ArgumentException("Asset order contains an image that does not belong to this project.");
        }

        string languageCode = orderedAssets.FirstOrDefault()?.LanguageCode ?? string.Empty;
        if (string.IsNullOrWhiteSpace(languageCode) || existingAssets.Count(asset => asset.LanguageCode == languageCode) != assetIds.Count || orderedAssets.Any(asset => asset!.LanguageCode != languageCode))
        {
            throw new ArgumentException("Asset order must include every image for one language exactly once.");
        }

        await _assetRepository.UpdateSortOrderAsync(projectId, assetIds, cancellationToken);
        var updatedAssets = await _assetRepository.ListByProjectAsync(projectId, cancellationToken);
        return MapAssets(updatedAssets);
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

    private AssetResponse MapAsset(Domain.Assets.Asset asset) => new(
        asset.Id,
        asset.OriginalFileName,
        asset.ContentType,
        asset.SizeBytes,
        _assetStorageService.GetPublicUrl(asset.StoredFileName),
        asset.LanguageCode,
        asset.SortOrder,
        asset.CreatedAt);

    private static void ValidateImage(IFormFile file)
    {
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException($"Unsupported file type '{file.ContentType}'.");
        }
    }
}
