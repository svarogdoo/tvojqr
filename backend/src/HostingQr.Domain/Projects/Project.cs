namespace HostingQr.Domain.Projects;

public sealed record Project(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string Status,
    string MenuType,
    string TimeZone,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
