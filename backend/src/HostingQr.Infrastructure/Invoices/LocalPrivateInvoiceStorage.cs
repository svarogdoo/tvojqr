using HostingQr.Application.Abstractions;
using HostingQr.Application.Invoices;
using HostingQr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Invoices;

public sealed class LocalPrivateInvoiceStorage : IPrivateInvoiceStorage
{
    private readonly string _rootPath;

    public LocalPrivateInvoiceStorage(IWebHostEnvironment environment, IOptions<PrivateInvoiceStorageOptions> options)
    {
        _rootPath = string.IsNullOrWhiteSpace(options.Value.RootPath)
            ? Path.Combine(environment.ContentRootPath, "data", "private-invoices")
            : Path.GetFullPath(options.Value.RootPath);
    }

    public async Task<StoredPrivateFile> SaveAsync(Guid invoiceId, Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);
        string key = $"{invoiceId:N}.pdf";
        string path = GetPath(key);
        await using FileStream output = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(output, cancellationToken);
        return new StoredPrivateFile(key, output.Length);
    }

    public Task<PrivateFile?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        string path = GetPath(storageKey);
        if (!File.Exists(path))
        {
            return Task.FromResult<PrivateFile?>(null);
        }

        FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult<PrivateFile?>(new PrivateFile(stream, "application/pdf", stream.Length));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        string path = GetPath(storageKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        return Task.CompletedTask;
    }

    private string GetPath(string storageKey)
    {
        if (!string.Equals(storageKey, Path.GetFileName(storageKey), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid private invoice storage key.");
        }
        return Path.Combine(_rootPath, storageKey);
    }
}
