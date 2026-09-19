using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Invoices;
using HostingQr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Invoices;

public sealed class R2PrivateInvoiceStorage : IPrivateInvoiceStorage, IDisposable
{
    private readonly PrivateR2Options _options;
    private readonly IAmazonS3 _client;

    public R2PrivateInvoiceStorage(IOptions<PrivateInvoiceStorageOptions> options)
    {
        _options = options.Value.R2;
        if (!_options.IsConfigured())
        {
            throw new InvalidOperationException("Private invoice R2 storage is enabled but configuration is incomplete.");
        }
        _client = new AmazonS3Client(new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey), new AmazonS3Config
        {
            ServiceURL = $"https://{_options.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
        });
    }

    public async Task<StoredPrivateFile> SaveAsync(Guid invoiceId, Stream content, CancellationToken cancellationToken = default)
    {
        string key = $"invoices/{invoiceId:N}.pdf";
        await using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = buffer,
            ContentType = "application/pdf",
            AutoCloseStream = false,
            UseChunkEncoding = false,
        }, cancellationToken);
        return new StoredPrivateFile(key, buffer.Length);
    }

    public async Task<PrivateFile?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            using GetObjectResponse response = await _client.GetObjectAsync(_options.BucketName, storageKey, cancellationToken);
            MemoryStream content = new();
            await response.ResponseStream.CopyToAsync(content, cancellationToken);
            content.Position = 0;
            return new PrivateFile(content, "application/pdf", content.Length);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) =>
        _client.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);

    public void Dispose() => _client.Dispose();
}
