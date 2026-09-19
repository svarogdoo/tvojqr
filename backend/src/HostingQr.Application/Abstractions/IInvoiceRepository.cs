using HostingQr.Application.Invoices;

namespace HostingQr.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<bool> ClientExistsAsync(Guid clientUserId, CancellationToken cancellationToken = default);
    Task<InvoiceRecord> CreateAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceRecord>> ListByClientAsync(Guid clientUserId, CancellationToken cancellationToken = default);
    Task<InvoiceRecord?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
