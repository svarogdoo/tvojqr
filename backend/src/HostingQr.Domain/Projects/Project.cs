namespace HostingQr.Domain.Projects;

public sealed record Project(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
