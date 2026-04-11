using HostingQr.Application.Abstractions;
using HostingQr.Application.Slugs;

namespace HostingQr.Api.Tests;

public sealed class SlugServiceTests
{
    [Fact]
    public async Task GenerateUniqueSlugAsync_ReturnsEightCharacterSlug()
    {
        SlugService service = new(new FakeSlugRepository());

        GeneratedSlugResponse result = await service.GenerateUniqueSlugAsync();

        Assert.Equal(8, result.Slug.Length);
        Assert.All(result.Slug, character => Assert.True(char.IsLower(character) || char.IsDigit(character)));
    }

    [Fact]
    public async Task CheckAvailabilityAsync_ReturnsFalseWhenSlugExists()
    {
        SlugService service = new(new FakeSlugRepository("taken123"));

        SlugAvailabilityResponse result = await service.CheckAvailabilityAsync("taken123");

        Assert.False(result.IsAvailable);
    }

    private sealed class FakeSlugRepository : ISlugRepository
    {
        private readonly HashSet<string> _existing;

        public FakeSlugRepository(params string[] existing)
        {
            _existing = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> ExistsAsync(string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_existing.Contains(slug));
        }
    }
}
