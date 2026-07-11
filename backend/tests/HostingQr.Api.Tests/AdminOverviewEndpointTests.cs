using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Admin;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HostingQr.Api.Tests;

public sealed class AdminOverviewEndpointTests
{
    [Fact]
    public async Task GetAdminOverview_ReturnsUnauthorizedWithoutAuthentication()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Migrations:RunOnStartup"] = "false",
                });
            });
        });
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        HttpResponseMessage response = await client.GetAsync("/api/admin/overview");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminOverview_ReturnsForbiddenForNonAdmin()
    {
        await using TestApplicationFactory factory = new(BillingTier.Standard);
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        HttpResponseMessage response = await client.GetAsync("/api/admin/overview");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminOverview_ReturnsMetricsForAdmin()
    {
        await using TestApplicationFactory factory = new(BillingTier.Admin);
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/admin/overview");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AdminOverviewResponse? payload = await response.Content.ReadFromJsonAsync<AdminOverviewResponse>();
        Assert.NotNull(payload);
        Assert.Equal(12, payload.TotalAccounts);
        Assert.Equal(345, payload.TotalViews);
        Assert.Equal(4, payload.AccountsByTier[BillingTier.Standard]);
        Assert.Equal(2, payload.AccountsByTier[BillingTier.Plus]);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _tier;

        public TestApplicationFactory(string tier)
        {
            _tier = tier;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEntitlementService>();
                services.RemoveAll<IAdminOverviewRepository>();
                services.AddSingleton<IEntitlementService>(new FakeEntitlementService(_tier));
                services.AddSingleton<IAdminOverviewRepository, FakeAdminOverviewRepository>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
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

    private sealed class FakeEntitlementService : IEntitlementService
    {
        private readonly string _tier;

        public FakeEntitlementService(string tier)
        {
            _tier = tier;
        }

        public Task<EntitlementResponse> GetCurrentEntitlementAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new EntitlementResponse(_tier, true, false, null));

        public Task<PlanLimits> GetCurrentPlanLimitsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlanLimitCatalog.ForTier(_tier));

        public Task<bool> CurrentUserHasToolAccessAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class FakeAdminOverviewRepository : IAdminOverviewRepository
    {
        public Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdminOverviewResponse(
                12,
                345,
                new Dictionary<string, long>
                {
                    [BillingTier.None] = 3,
                    [BillingTier.Admin] = 1,
                    [BillingTier.Free] = 2,
                    [BillingTier.Standard] = 4,
                    [BillingTier.Plus] = 2,
                }));
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
                new Claim(ClaimTypes.Email, "admin@hostingqr.local"),
                new Claim(ClaimTypes.Name, "Admin User"),
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
