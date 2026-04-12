namespace HostingQr.Application.Abstractions;

public interface IAssetStorageService
{
    Task<StoredAssetFile> SaveImageAsync(Guid projectId, Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default);

    string GetPublicUrl(string storedFileName);
}

public sealed record StoredAssetFile(string StoredFileName, string ContentType, long SizeBytes);
