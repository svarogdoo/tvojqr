using HostingQr.Application.Invoices;

namespace HostingQr.Application.Abstractions;

public interface IPrivateInvoiceStorage
{
    Task<StoredPrivateFile> SaveAsync(Guid invoiceId, Stream content, CancellationToken cancellationToken = default);
    Task<PrivateFile?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
