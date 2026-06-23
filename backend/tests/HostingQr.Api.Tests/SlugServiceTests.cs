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

    [Theory]
    [InlineData("dashboard")]
    [InlineData("pricing")]
    [InlineData("contact")]
    [InlineData("privacy")]
    [InlineData("terms")]
    [InlineData("api")]
    public void NormalizeOrThrow_RejectsReservedAppRoutes(string slug)
    {
        SlugService service = new(new FakeSlugRepository());

        ArgumentException exception = Assert.Throws<ArgumentException>(() => service.NormalizeOrThrow(slug));

        Assert.Contains("reserved", exception.Message, StringComparison.OrdinalIgnoreCase);
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

        public Task<bool> ExistsForOtherProjectAsync(string slug, Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_existing.Contains(slug));
        }
    }
}
