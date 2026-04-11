using System.Net;
using System.Net.Http.Json;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Projects;
using HostingQr.Application.Slugs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HostingQr.Api.Tests;

public sealed class ProjectEndpointTests
{
    [Fact]
    public async Task GetProjects_ReturnsProjectList()
    {
        await using TestApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ProjectListItem>? payload = await response.Content.ReadFromJsonAsync<List<ProjectListItem>>();
        Assert.NotNull(payload);
        Assert.Single(payload);
        Assert.Equal("summer-menu", payload[0].Slug);
    }

    [Fact]
    public async Task PostGenerateSlug_ReturnsGeneratedSlug()
    {
        await using TestApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/api/slugs/generate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GeneratedSlugResponse? payload = await response.Content.ReadFromJsonAsync<GeneratedSlugResponse>();
        Assert.NotNull(payload);
        Assert.Equal("randm123", payload.Slug);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IProjectService>();
                services.RemoveAll<ISlugService>();
                services.AddScoped<IProjectService, FakeProjectService>();
                services.AddScoped<ISlugService, FakeSlugService>();
            });

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Migrations:RunOnStartup"] = "false",
                });
            });
        }
    }

    private sealed class FakeProjectService : IProjectService
    {
        public Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ProjectDetailResponse(Guid.NewGuid(), request.Name, request.Slug, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        }

        public Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProjectDetailResponse?>(new ProjectDetailResponse(projectId, "Summer Menu", "summer-menu", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        }

        public Task<PublicProjectResponse?> GetPublicProjectAsync(string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PublicProjectResponse?>(new PublicProjectResponse(Guid.NewGuid(), "Summer Menu", slug, "Demo User"));
        }

        public Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ProjectListItem> items = [new ProjectListItem(Guid.NewGuid(), "Summer Menu", "summer-menu", DateTimeOffset.UtcNow)];
            return Task.FromResult(items);
        }
    }

    private sealed class FakeSlugService : ISlugService
    {
        public Task<SlugAvailabilityResponse> CheckAvailabilityAsync(string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SlugAvailabilityResponse(slug, slug != "taken123"));
        }

        public Task<GeneratedSlugResponse> GenerateUniqueSlugAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new GeneratedSlugResponse("randm123"));
        }

        public string NormalizeOrThrow(string slug) => slug.Trim().ToLowerInvariant();
    }
}
