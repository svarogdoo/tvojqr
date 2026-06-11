namespace HostingQr.Application.Assets;

public sealed record AssetResponse(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Url,
    string LanguageCode,
    int SortOrder,
    DateTimeOffset CreatedAt);

public sealed record ReorderAssetsRequest(IReadOnlyList<Guid> AssetIds);
