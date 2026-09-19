using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Application.Invoices;

public sealed class InvoiceService
{
    public const long MaxInvoiceSizeBytes = 10 * 1024 * 1024;

    private readonly ICurrentUserContext _currentUserContext;
    private readonly IProjectAccessService _projectAccessService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPrivateInvoiceStorage _storage;

    public InvoiceService(
        ICurrentUserContext currentUserContext,
        IProjectAccessService projectAccessService,
        IInvoiceRepository invoiceRepository,
        IPrivateInvoiceStorage storage)
    {
        _currentUserContext = currentUserContext;
        _projectAccessService = projectAccessService;
        _invoiceRepository = invoiceRepository;
        _storage = storage;
    }

    public async Task<InvoiceResponse> UploadAsync(Guid clientUserId, DateOnly invoiceDate, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (!await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        if (!await _invoiceRepository.ClientExistsAsync(clientUserId, cancellationToken))
        {
            throw new KeyNotFoundException("Client account was not found.");
        }

        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invoice must be a PDF file.");
        }

        if (file.Length is <= 0 or > MaxInvoiceSizeBytes)
        {
            throw new ArgumentException("Invoice PDF must be between 1 byte and 10 MB.");
        }

        Guid invoiceId = Guid.NewGuid();
        await using MemoryStream verifiedContent = new();
        await using (Stream input = file.OpenReadStream())
        {
            await input.CopyToAsync(verifiedContent, cancellationToken);
        }
        ReadOnlySpan<byte> signature = verifiedContent.GetBuffer().AsSpan(0, Math.Min(5, checked((int)verifiedContent.Length)));
        if (!signature.SequenceEqual("%PDF-"u8))
        {
            throw new ArgumentException("Invoice file content is not a valid PDF.");
        }
        verifiedContent.Position = 0;
        StoredPrivateFile stored = await _storage.SaveAsync(invoiceId, verifiedContent, cancellationToken);

        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            InvoiceRecord invoice = await _invoiceRepository.CreateAsync(new InvoiceRecord(
                invoiceId,
                clientUserId,
                _currentUserContext.GetCurrentUserId(),
                invoiceDate,
                Path.GetFileName(file.FileName),
                "application/pdf",
                stored.StorageKey,
                stored.SizeBytes,
                now,
                now), cancellationToken);
            return Map(invoice);
        }
        catch
        {
            await _storage.DeleteAsync(stored.StorageKey, CancellationToken.None);
            throw;
        }
    }

    public async Task<IReadOnlyList<InvoiceResponse>> ListAsync(Guid? clientUserId, CancellationToken cancellationToken = default)
    {
        bool isAdmin = await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken);
        Guid currentUserId = _currentUserContext.GetCurrentUserId();
        Guid targetUserId = clientUserId ?? currentUserId;
        if (!isAdmin && targetUserId != currentUserId)
        {
            throw new UnauthorizedAccessException();
        }

        return (await _invoiceRepository.ListByClientAsync(targetUserId, cancellationToken)).Select(Map).ToArray();
    }

    public async Task<(InvoiceRecord Invoice, PrivateFile File)?> DownloadAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        InvoiceRecord? invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        Guid currentUserId = _currentUserContext.GetCurrentUserId();
        if (invoice.ClientUserId != currentUserId && !await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        PrivateFile? file = await _storage.OpenReadAsync(invoice.StorageKey, cancellationToken);
        return file is null ? null : (invoice, file);
    }

    public async Task<bool> DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        if (!await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        InvoiceRecord? invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return false;
        }

        await _storage.DeleteAsync(invoice.StorageKey, cancellationToken);
        return await _invoiceRepository.DeleteAsync(invoiceId, cancellationToken);
    }

    private static InvoiceResponse Map(InvoiceRecord invoice) => new(
        invoice.Id,
        invoice.ClientUserId,
        invoice.InvoiceDate,
        invoice.OriginalFileName,
        invoice.ContentType,
        invoice.SizeBytes,
        invoice.CreatedAt);
}
