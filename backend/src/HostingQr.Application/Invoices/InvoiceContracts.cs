namespace HostingQr.Application.Invoices;

public sealed record InvoiceResponse(
    Guid Id,
    Guid ClientUserId,
    DateOnly InvoiceDate,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt);

public sealed record InvoiceRecord(
    Guid Id,
    Guid ClientUserId,
    Guid UploadedByUserId,
    DateOnly InvoiceDate,
    string OriginalFileName,
    string ContentType,
    string StorageKey,
    long SizeBytes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record StoredPrivateFile(string StorageKey, long SizeBytes);

public sealed record PrivateFile(Stream Content, string ContentType, long? Length) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
