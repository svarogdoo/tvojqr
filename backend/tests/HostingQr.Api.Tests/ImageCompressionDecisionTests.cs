using HostingQr.Infrastructure.Assets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace HostingQr.Api.Tests;

public sealed class ImageCompressionDecisionTests
{
    [Fact]
    public async Task PrepareAsync_KeepsSmallOriginalImage()
    {
        byte[] bytes = await CreatePngAsync(64, 64);
        using MemoryStream stream = new(bytes);

        PreparedImageAsset prepared = await ImageCompressionDecision.PrepareAsync(stream, "tiny.png", "image/png");

        Assert.Equal("image/png", prepared.ContentType);
        Assert.Equal(".png", prepared.Extension);
        Assert.Equal(bytes.Length, prepared.Bytes.Length);
    }

    [Fact]
    public async Task PrepareAsync_UsesWebpWhenLargeImageShrinksEnough()
    {
        byte[] bytes = await CreateJpegAsync(1400, 1400);
        using MemoryStream stream = new(bytes);

        PreparedImageAsset prepared = await ImageCompressionDecision.PrepareAsync(stream, "large.jpg", "image/jpeg");

        Assert.Equal("image/webp", prepared.ContentType);
        Assert.Equal(".webp", prepared.Extension);
        Assert.True(prepared.Bytes.Length < bytes.Length * 0.85);
    }

    private static async Task<byte[]> CreatePngAsync(int width, int height)
    {
        using Image<Rgba32> image = new(width, height);
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgba32((byte)(x % 255), (byte)(y % 255), (byte)((x + y) % 255));
                }
            }
        });

        await using MemoryStream stream = new();
        await image.SaveAsPngAsync(stream, new PngEncoder());
        return stream.ToArray();
    }

    private static async Task<byte[]> CreateJpegAsync(int width, int height)
    {
        using Image<Rgba32> image = new(width, height);
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgba32((byte)(x % 255), (byte)(y % 255), (byte)((x + y) % 255));
                }
            }
        });

        await using MemoryStream stream = new();
        await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = 100 });
        return stream.ToArray();
    }
}
