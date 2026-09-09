namespace HostingQr.Domain.Projects;

public sealed record PublicProject
{
    public Guid ProjectId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string OwnerDisplayName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string MenuType { get; init; } = ProjectMenuType.Image;

    public string TimeZone { get; init; } = "UTC";

    public string BackgroundColor { get; init; } = string.Empty;
}
