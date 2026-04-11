namespace HostingQr.Domain.Slugs;

public sealed record Slug(
    Guid Id,
    Guid ProjectId,
    string Value,
    bool IsPrimary,
    DateTimeOffset CreatedAt);
