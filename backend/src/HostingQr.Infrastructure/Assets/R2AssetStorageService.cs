using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace HostingQr.Infrastructure.Assets;

public sealed class R2AssetStorageService : IAssetStorageService
{
    private readonly R2StorageOptions _r2Options;
    private readonly IAmazonS3 _s3Client;

    public R2AssetStorageService(IOptions<StorageOptions> storageOptions)
    {
        _r2Options = storageOptions.Value.R2;
        if (!_r2Options.IsConfigured())
        {
            throw new InvalidOperationException("R2 storage is enabled but Storage:R2 configuration is incomplete.");
        }

        var credentials = new BasicAWSCredentials(_r2Options.AccessKeyId, _r2Options.SecretAccessKey);
        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{_r2Options.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
        };

        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<StoredAssetFile> SaveImageAsync(Guid projectId, Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        string objectKey = $"projects/{projectId:N}/languages/default/{Guid.NewGuid():N}.webp";

        await using MemoryStream output = new();
        using Image image = await Image.LoadAsync(stream, cancellationToken);
        WebpEncoder encoder = new()
        {
            Quality = 80,
            FileFormat = WebpFileFormatType.Lossy,
        };

        await image.SaveAsWebpAsync(output, encoder, cancellationToken);
        output.Position = 0;

        PutObjectRequest request = new()
        {
            BucketName = _r2Options.BucketName,
            Key = objectKey,
            InputStream = output,
            ContentType = "image/webp",
            AutoCloseStream = false,
            UseChunkEncoding = false,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return new StoredAssetFile(objectKey, "image/webp", output.Length);
    }

    public string GetPublicUrl(string storedFileName)
    {
        if (Uri.TryCreate(storedFileName, UriKind.Absolute, out _))
        {
            return storedFileName;
        }

        string publicBaseUrl = _r2Options.PublicBaseUrl!.TrimEnd('/');
        return $"{publicBaseUrl}/{storedFileName}";
    }

    public async Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        DeleteObjectRequest request = new()
        {
            BucketName = _r2Options.BucketName,
            Key = storedFileName,
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }
}
