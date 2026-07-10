using System.Net;
using System.Security.Cryptography;
using System.Text;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HostingQr.Api.Tests;

public sealed class BillingWebhookEndpointTests
{
    private const string WebhookSecret = "test-webhook-secret";

    [Fact]
    public async Task PolarWebhook_RejectsInvalidSignature()
    {
        await using TestApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        using StringContent content = new("{}", Encoding.UTF8, "application/json");
        content.Headers.Add("webhook-id", "evt_test");
        content.Headers.Add("webhook-timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        content.Headers.Add("webhook-signature", "v1,invalid");

        HttpResponseMessage response = await client.PostAsync("/api/billing/polar/webhook", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(factory.EntitlementRepository.LastUpsert);
    }

    [Fact]
    public async Task PolarWebhook_ActivatesEntitlementForPaidOrder()
    {
        await using TestApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();
        string payload = """
            {
              "type": "order.paid",
              "data": {
                "metadata": {
                  "userId": "11111111-1111-1111-1111-111111111111",
                  "tier": "standard",
                  "billingCycle": "monthly"
                }
              }
            }
            """;
        using StringContent content = CreateSignedContent(payload);

        HttpResponseMessage response = await client.PostAsync("/api/billing/polar/webhook", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(factory.EntitlementRepository.LastUpsert);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), factory.EntitlementRepository.LastUpsert.UserId);
        Assert.Equal(BillingTier.Standard, factory.EntitlementRepository.LastUpsert.Tier);
        Assert.True(factory.EntitlementRepository.LastUpsert.IsActive);
    }

    private static StringContent CreateSignedContent(string payload)
    {
        string id = "evt_test";
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string signedContent = $"{id}.{timestamp}.{payload}";
        string signature = Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(WebhookSecret), Encoding.UTF8.GetBytes(signedContent)));

        StringContent content = new(payload, Encoding.UTF8, "application/json");
        content.Headers.Add("webhook-id", id);
        content.Headers.Add("webhook-timestamp", timestamp);
        content.Headers.Add("webhook-signature", $"v1,{signature}");
        return content;
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        public FakeEntitlementRepository EntitlementRepository { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEntitlementRepository>();
                services.AddSingleton<IEntitlementRepository>(EntitlementRepository);
            });

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Migrations:RunOnStartup"] = "false",
                    ["Polar:WebhookSecret"] = WebhookSecret,
                });
            });
        }
    }

    public sealed class FakeEntitlementRepository : IEntitlementRepository
    {
        public EntitlementUpsert? LastUpsert { get; private set; }

        public Task<EntitlementResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EntitlementResponse(BillingTier.None, false, false, null));
        }

        public Task UpsertAsync(Guid userId, string tier, bool isActive, DateTimeOffset? endsAt, bool grantedManually = false, CancellationToken cancellationToken = default)
        {
            LastUpsert = new EntitlementUpsert(userId, tier, isActive, endsAt, grantedManually);
            return Task.CompletedTask;
        }
    }

    public sealed record EntitlementUpsert(Guid UserId, string Tier, bool IsActive, DateTimeOffset? EndsAt, bool GrantedManually);
}
