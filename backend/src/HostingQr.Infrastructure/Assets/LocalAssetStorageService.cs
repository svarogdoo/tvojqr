using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

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
        string storedFileName = $"{projectId:N}-{Guid.NewGuid():N}.webp";
        string webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        string uploadsPath = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);
        string fullPath = Path.Combine(uploadsPath, storedFileName);

        using Image image = await Image.LoadAsync(stream, cancellationToken);
        WebpEncoder encoder = new()
        {
            Quality = 80,
            FileFormat = WebpFileFormatType.Lossy,
        };

        await using FileStream output = File.Create(fullPath);
        await image.SaveAsWebpAsync(output, encoder, cancellationToken);
        await output.FlushAsync(cancellationToken);

        return new StoredAssetFile(storedFileName, "image/webp", output.Length);
    }

    public string GetPublicUrl(string storedFileName) => $"/uploads/{storedFileName}";
}
