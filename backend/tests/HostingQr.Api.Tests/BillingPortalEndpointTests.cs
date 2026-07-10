using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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

public sealed class BillingPortalEndpointTests
{
    [Fact]
    public async Task CreateBillingPortal_ReturnsUnauthorizedWithoutAuthentication()
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

        HttpResponseMessage response = await client.PostAsync("/api/billing/portal", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBillingPortal_ReturnsPolarPortalUrl()
    {
        await using TestApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/api/billing/portal", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        BillingPortalResponse? payload = await response.Content.ReadFromJsonAsync<BillingPortalResponse>();
        Assert.NotNull(payload);
        Assert.Equal("https://polar.sh/test-org/portal/session-token", payload.PortalUrl);
        Assert.Equal("https://sandbox-api.polar.sh/v1/customer-sessions/", factory.HttpClientFactory.LastRequestUri?.ToString());
        Assert.Contains("11111111-1111-1111-1111-111111111111", factory.HttpClientFactory.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("http://localhost:5173/account", factory.HttpClientFactory.LastRequestBody, StringComparison.Ordinal);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        public FakeHttpClientFactory HttpClientFactory { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory>(HttpClientFactory);
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
                    ["Polar:AccessToken"] = "test-token",
                    ["Polar:BaseUrl"] = "https://sandbox-api.polar.sh",
                    ["Auth:FrontendBaseUrl"] = "http://localhost:5173",
                });
            });
        }
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly FakePolarHandler _handler = new();

        public Uri? LastRequestUri => _handler.LastRequestUri;

        public string LastRequestBody => _handler.LastRequestBody;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class FakePolarHandler : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastRequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""
                    {
                      "customer_portal_url": "https://polar.sh/test-org/portal/session-token"
                    }
                    """, Encoding.UTF8, "application/json")
            };
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

    private sealed record BillingPortalResponse(string PortalUrl);
}
