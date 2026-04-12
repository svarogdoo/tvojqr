using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
using HostingQr.Application.Auth;
using HostingQr.Application.Projects;
using HostingQr.Application.Slugs;
using Microsoft.AspNetCore.Http;
using HostingQr.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HostingQr.Api.Tests;

public sealed class ProjectEndpointTests
{
    [Fact]
    public async Task GetProjects_ReturnsUnauthorizedWithoutAuthentication()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        HttpResponseMessage response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ReturnsProjectList()
    {
        await using TestApplicationFactory factory = new(authenticated: true);
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
        await using TestApplicationFactory factory = new(authenticated: true);
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/api/slugs/generate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GeneratedSlugResponse? payload = await response.Content.ReadFromJsonAsync<GeneratedSlugResponse>();
        Assert.NotNull(payload);
        Assert.Equal("randm123", payload.Slug);
    }

    [Fact]
    public async Task PutProject_ReturnsUpdatedProject()
    {
        await using TestApplicationFactory factory = new(authenticated: true);
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync(
            "/api/projects/11111111-1111-1111-1111-111111111111",
            new UpdateProjectRequest("Updated Menu", "updated-menu"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProjectDetailResponse? payload = await response.Content.ReadFromJsonAsync<ProjectDetailResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Updated Menu", payload.Name);
        Assert.Equal("updated-menu", payload.Slug);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsAuthenticatedUser()
    {
        await using TestApplicationFactory factory = new(authenticated: true);
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthUserResponse? payload = await response.Content.ReadFromJsonAsync<AuthUserResponse>();
        Assert.NotNull(payload);
        Assert.Equal("demo@hostingqr.local", payload.Email);
    }

    [Fact]
    public async Task PostProjectAssets_ReturnsUploadedAssets()
    {
        await using TestApplicationFactory factory = new(authenticated: true);
        HttpClient client = factory.CreateClient();

        using MultipartFormDataContent form = new();
        form.Add(new ByteArrayContent([1, 2, 3]), "files", "menu.png");

        HttpResponseMessage response = await client.PostAsync("/api/projects/11111111-1111-1111-1111-111111111111/assets", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<AssetResponse>? payload = await response.Content.ReadFromJsonAsync<List<AssetResponse>>();
        Assert.NotNull(payload);
        Assert.Single(payload);
        Assert.Equal("menu.png", payload[0].OriginalFileName);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly bool _authenticated;

        public TestApplicationFactory(bool authenticated)
        {
            _authenticated = authenticated;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IProjectService>();
                services.RemoveAll<ISlugService>();
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IAssetService>();
                services.AddScoped<IProjectService, FakeProjectService>();
                services.AddScoped<ISlugService, FakeSlugService>();
                services.AddScoped<IUserRepository, FakeUserRepository>();
                services.AddScoped<IAssetService, FakeAssetService>();

                if (_authenticated)
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
                }
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

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<AuthUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AuthUserResponse?>(new AuthUserResponse(id, "demo@hostingqr.local", "Demo User"));
        }

        public Task<AuthUserResponse> UpsertAsync(Guid id, string email, string displayName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuthUserResponse(id, email, displayName));
        }
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestAuth";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(AuthConstants.UserIdClaimType, "11111111-1111-1111-1111-111111111111"),
                new Claim(ClaimTypes.Email, "demo@hostingqr.local"),
                new Claim(ClaimTypes.Name, "Demo User"),
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed class FakeProjectService : IProjectService
    {
        public Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ProjectDetailResponse(Guid.NewGuid(), request.Name, request.Slug, "active", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []));
        }

        public Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AssetResponse> assets =
            [
                new AssetResponse(Guid.NewGuid(), "existing-menu.png", "image/png", 1234, "/uploads/existing-menu.png", "default", 0, DateTimeOffset.UtcNow),
            ];

            return Task.FromResult<ProjectDetailResponse?>(new ProjectDetailResponse(projectId, "Summer Menu", "summer-menu", "active", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, assets));
        }

        public Task<ProjectDetailResponse?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProjectDetailResponse?>(new ProjectDetailResponse(projectId, request.Name, request.Slug, "active", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []));
        }

        public Task<ProjectDetailResponse?> UpdateProjectStatusAsync(Guid projectId, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProjectDetailResponse?>(new ProjectDetailResponse(projectId, "Summer Menu", "summer-menu", request.Status, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []));
        }

        public Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<PublicProjectResponse?> GetPublicProjectAsync(string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PublicProjectResponse?>(new PublicProjectResponse(Guid.NewGuid(), "Summer Menu", slug, "Demo User", "active", []));
        }

        public Task<IReadOnlyList<ProjectListItem>> ListProjectsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ProjectListItem> items = [new ProjectListItem(Guid.NewGuid(), "Summer Menu", "summer-menu", "active", DateTimeOffset.UtcNow)];
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

        public Task<bool> ExistsForOtherProjectAsync(string slug, Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public string NormalizeOrThrow(string slug) => slug.Trim().ToLowerInvariant();
    }

    private sealed class FakeAssetService : IAssetService
    {
        public IReadOnlyList<AssetResponse> MapAssets(IReadOnlyList<HostingQr.Domain.Assets.Asset> assets) => [];

        public Task<IReadOnlyList<AssetResponse>> UploadImagesAsync(Guid projectId, IFormFileCollection files, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AssetResponse> assets =
            [
                new AssetResponse(Guid.NewGuid(), "menu.png", "image/png", 3, "/uploads/test-menu.png", "default", 0, DateTimeOffset.UtcNow),
            ];

            return Task.FromResult(assets);
        }
    }
}
