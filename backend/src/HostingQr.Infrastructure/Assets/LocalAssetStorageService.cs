using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace HostingQr.Infrastructure.Assets;

public sealed class LocalAssetStorageService : IAssetStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalAssetStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<StoredAssetFile> SaveImageAsync(Guid projectId, Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(originalFileName);
        string storedFileName = $"{projectId:N}-{Guid.NewGuid():N}{extension}";
        string webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        string uploadsPath = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);
        string fullPath = Path.Combine(uploadsPath, storedFileName);

        await using FileStream output = File.Create(fullPath);
        await stream.CopyToAsync(output, cancellationToken);
        await output.FlushAsync(cancellationToken);

        return new StoredAssetFile(storedFileName, contentType, output.Length);
    }

    public string GetPublicUrl(string storedFileName) => $"/uploads/{storedFileName}";
}
