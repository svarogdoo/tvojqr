using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Assets;

public sealed class LocalAssetStorageService : IAssetStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly StorageOptions _storageOptions;

    public LocalAssetStorageService(IWebHostEnvironment environment, IOptions<StorageOptions> storageOptions)
    {
        _environment = environment;
        _storageOptions = storageOptions.Value;
    }

    public async Task<StoredAssetFile> SaveImageAsync(Guid projectId, Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        PreparedImageAsset prepared = await ImageCompressionDecision.PrepareAsync(stream, originalFileName, contentType, cancellationToken);
        string storedFileName = $"{projectId:N}-{Guid.NewGuid():N}{prepared.Extension}";
        string uploadsPath = GetUploadsPath();
        Directory.CreateDirectory(uploadsPath);
        string fullPath = Path.Combine(uploadsPath, storedFileName);

        await File.WriteAllBytesAsync(fullPath, prepared.Bytes, cancellationToken);

        return new StoredAssetFile(storedFileName, prepared.ContentType, prepared.Bytes.Length);
    }

    public string GetPublicUrl(string storedFileName) => $"/uploads/{storedFileName}";

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.Combine(GetUploadsPath(), storedFileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetUploadsPath()
    {
        if (!string.IsNullOrWhiteSpace(_storageOptions.UploadsRootPath))
        {
            return _storageOptions.UploadsRootPath;
        }

        string webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        return Path.Combine(webRootPath, "uploads");
    }
}
