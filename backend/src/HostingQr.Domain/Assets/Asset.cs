namespace HostingQr.Domain.Assets;

public sealed record Asset
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string OriginalFileName { get; init; } = string.Empty;

    public string StoredFileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long SizeBytes { get; init; }

    public int SortOrder { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
