using HostingQr.Application.Assets;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Application.Abstractions;

public interface IAssetService
{
    Task<IReadOnlyList<AssetResponse>> UploadImagesAsync(Guid projectId, IFormFileCollection files, CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(Guid projectId, Guid assetId, CancellationToken cancellationToken = default);

    IReadOnlyList<AssetResponse> MapAssets(IReadOnlyList<Domain.Assets.Asset> assets);
}
