namespace HostingQr.Domain.Projects;

public sealed record ProjectLanguageVariant
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsDefault { get; init; }

    public int SortOrder { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
