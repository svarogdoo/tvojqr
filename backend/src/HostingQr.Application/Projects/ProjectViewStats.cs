namespace HostingQr.Application.Projects;

public sealed record ProjectViewStats(Guid ProjectId, long TotalViews, DateTimeOffset? LastViewedAt);
