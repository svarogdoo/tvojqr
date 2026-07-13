using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace HostingQr.Infrastructure.Assets;

public sealed record PreparedImageAsset(byte[] Bytes, string ContentType, string Extension);

public static class ImageCompressionDecision
{
    private const long SkipCompressionUnderBytes = 250_000;
    private const decimal MinimumSavingsRatio = 0.15m;

    public static async Task<PreparedImageAsset> PrepareAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        await using MemoryStream original = new();
        await stream.CopyToAsync(original, cancellationToken);
        byte[] originalBytes = original.ToArray();
        string originalExtension = GetSafeExtension(originalFileName, contentType);

        if (originalBytes.Length < SkipCompressionUnderBytes)
        {
            return new PreparedImageAsset(originalBytes, contentType, originalExtension);
        }

        original.Position = 0;
        await using MemoryStream webp = new();
        using Image image = await Image.LoadAsync(original, cancellationToken);
        WebpEncoder encoder = new()
        {
            Quality = 90,
            FileFormat = WebpFileFormatType.Lossy,
        };

        await image.SaveAsWebpAsync(webp, encoder, cancellationToken);
        byte[] webpBytes = webp.ToArray();
        decimal savingsRatio = 1m - ((decimal)webpBytes.Length / originalBytes.Length);

        return savingsRatio >= MinimumSavingsRatio
            ? new PreparedImageAsset(webpBytes, "image/webp", ".webp")
            : new PreparedImageAsset(originalBytes, contentType, originalExtension);
    }

    private static string GetSafeExtension(string originalFileName, string contentType)
    {
        string extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" => extension,
            _ => contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".img",
            },
        };
    }
}
