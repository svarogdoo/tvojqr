namespace HostingQr.Domain.Projects;

public sealed record Project(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
