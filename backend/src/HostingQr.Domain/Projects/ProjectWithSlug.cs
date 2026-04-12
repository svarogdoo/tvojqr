namespace HostingQr.Domain.Projects;

public sealed record ProjectWithSlug
{
    public Guid Id { get; init; }

    public Guid OwnerUserId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
