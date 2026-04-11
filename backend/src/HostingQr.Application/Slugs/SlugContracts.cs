namespace HostingQr.Application.Slugs;

public sealed record SlugAvailabilityResponse(string Slug, bool IsAvailable);

public sealed record GeneratedSlugResponse(string Slug);
