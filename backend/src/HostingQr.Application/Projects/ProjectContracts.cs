using HostingQr.Application.Assets;

namespace HostingQr.Application.Projects;

public sealed record ProjectListItem(Guid Id, string Name, string Slug, string Status, DateTimeOffset UpdatedAt);

public sealed record ProjectDetailResponse(Guid Id, string Name, string Slug, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IReadOnlyList<AssetResponse> Assets);

public sealed record CreateProjectRequest(string Name, string Slug);

public sealed record UpdateProjectRequest(string Name, string Slug);

public sealed record UpdateProjectStatusRequest(string Status);

public sealed record PublicProjectResponse(Guid ProjectId, string Name, string Slug, string OwnerDisplayName, string Status, IReadOnlyList<AssetResponse> Assets);
