namespace HostingQr.Domain.Projects;

public sealed record ProjectWithSlug(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
