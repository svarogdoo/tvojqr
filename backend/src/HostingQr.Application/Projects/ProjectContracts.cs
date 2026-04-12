namespace HostingQr.Application.Projects;

public sealed record ProjectListItem(Guid Id, string Name, string Slug, DateTimeOffset UpdatedAt);

public sealed record ProjectDetailResponse(Guid Id, string Name, string Slug, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record CreateProjectRequest(string Name, string Slug);

public sealed record UpdateProjectRequest(string Name, string Slug);

public sealed record PublicProjectResponse(Guid ProjectId, string Name, string Slug, string OwnerDisplayName);
