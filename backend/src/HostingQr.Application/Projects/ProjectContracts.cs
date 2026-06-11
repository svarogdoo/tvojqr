using HostingQr.Application.Assets;

namespace HostingQr.Application.Projects;

public sealed record ProjectListItem(Guid Id, string Name, string Slug, string Status, DateTimeOffset UpdatedAt);

public sealed record ProjectDetailResponse(Guid Id, string Name, string Slug, string Status, string BackgroundColor, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IReadOnlyList<ProjectLanguageVariantResponse> Languages, IReadOnlyList<AssetResponse> Assets);

public sealed record CreateProjectRequest(string Name, string Slug, string? BackgroundColor);

public sealed record UpdateProjectRequest(string Name, string Slug, string? BackgroundColor);

public sealed record UpdateProjectStatusRequest(string Status);

public sealed record ProjectLanguageVariantResponse(Guid Id, string LanguageCode, string DisplayName, bool IsDefault, int SortOrder);

public sealed record CreateProjectLanguageRequest(string LanguageCode, string DisplayName);

public sealed record PublicProjectResponse(Guid ProjectId, string Name, string Slug, string OwnerDisplayName, string Status, string BackgroundColor, IReadOnlyList<ProjectLanguageVariantResponse> Languages, IReadOnlyList<AssetResponse> Assets);
