namespace HostingQr.Domain.Projects;

public sealed record PublicProject(
    Guid ProjectId,
    string Name,
    string Slug,
    string OwnerDisplayName);
