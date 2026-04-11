using System.Security.Cryptography;
using HostingQr.Application.Abstractions;

namespace HostingQr.Application.Slugs;

public sealed class SlugService : ISlugService
{
    private const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int GeneratedSlugLength = 8;
    private readonly ISlugRepository _slugRepository;

    public SlugService(ISlugRepository slugRepository)
    {
        _slugRepository = slugRepository;
    }

    public async Task<SlugAvailabilityResponse> CheckAvailabilityAsync(string slug, CancellationToken cancellationToken = default)
    {
        string normalized = NormalizeOrThrow(slug);
        bool exists = await _slugRepository.ExistsAsync(normalized, cancellationToken);
        return new SlugAvailabilityResponse(normalized, !exists);
    }

    public async Task<GeneratedSlugResponse> GenerateUniqueSlugAsync(CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            string slug = GenerateRandomSlug();
            if (!await _slugRepository.ExistsAsync(slug, cancellationToken))
            {
                return new GeneratedSlugResponse(slug);
            }
        }

        throw new InvalidOperationException("Unable to generate a unique slug right now.");
    }

    public string NormalizeOrThrow(string slug)
    {
        string normalized = slug.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 50)
        {
            throw new ArgumentException("Slug must be between 3 and 50 characters.", nameof(slug));
        }

        if (normalized.Any(ch => !(char.IsLower(ch) || char.IsDigit(ch) || ch == '-')))
        {
            throw new ArgumentException("Slug can only contain lowercase letters, numbers, and hyphens.", nameof(slug));
        }

        if (normalized.StartsWith('-') || normalized.EndsWith('-') || normalized.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug format is invalid.", nameof(slug));
        }

        return normalized;
    }

    private static string GenerateRandomSlug()
    {
        Span<char> chars = stackalloc char[GeneratedSlugLength];
        Span<byte> bytes = stackalloc byte[GeneratedSlugLength];
        RandomNumberGenerator.Fill(bytes);

        for (int i = 0; i < GeneratedSlugLength; i++)
        {
            chars[i] = AllowedCharacters[bytes[i] % AllowedCharacters.Length];
        }

        return new string(chars);
    }
}
