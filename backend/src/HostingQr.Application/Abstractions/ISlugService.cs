using HostingQr.Application.Slugs;

namespace HostingQr.Application.Abstractions;

public interface ISlugService
{
    Task<SlugAvailabilityResponse> CheckAvailabilityAsync(string slug, CancellationToken cancellationToken = default);

    Task<GeneratedSlugResponse> GenerateUniqueSlugAsync(CancellationToken cancellationToken = default);

    string NormalizeOrThrow(string slug);
}
