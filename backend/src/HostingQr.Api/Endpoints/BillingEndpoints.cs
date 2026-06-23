using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HostingQr.Api.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/billing").WithTags("Billing");

        group.MapGet("/entitlement", [Authorize] async (IEntitlementService entitlementService, CancellationToken cancellationToken) =>
            Results.Ok(await entitlementService.GetCurrentEntitlementAsync(cancellationToken)))
            .WithName("GetCurrentEntitlement")
            .WithSummary("Returns the current user's active pricing tier entitlement.");

        group.MapPost("/checkout", [Authorize] async (
            CheckoutRequest request,
            ICurrentUserContext currentUserContext,
            IHttpClientFactory httpClientFactory,
            IOptions<PolarOptions> polarOptions,
            CancellationToken cancellationToken) =>
        {
            PolarOptions options = polarOptions.Value;
            if (!options.IsConfigured())
            {
                return Results.Problem(
                    "Polar checkout is not configured.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            string normalizedTier = NormalizeTier(request.Tier);
            string normalizedCycle = NormalizeBillingCycle(request.BillingCycle);
            string? productId = GetProductId(options.Products, normalizedTier, normalizedCycle);
            if (productId is null)
            {
                return Results.BadRequest(new { error = "Unsupported checkout plan." });
            }

            CurrentUser currentUser = currentUserContext.GetCurrentUser();
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);

            PolarCheckoutCreate polarRequest = new(
                [productId],
                options.SuccessUrl,
                options.CancelUrl,
                currentUser.Id.ToString(),
                currentUser.Email,
                currentUser.DisplayName,
                new Dictionary<string, string>
                {
                    ["userId"] = currentUser.Id.ToString(),
                    ["tier"] = normalizedTier,
                    ["billingCycle"] = normalizedCycle
                });

            HttpResponseMessage polarResponse = await httpClient.PostAsJsonAsync("v1/checkouts/", polarRequest, cancellationToken);
            if (!polarResponse.IsSuccessStatusCode)
            {
                return Results.Problem(
                    "Polar checkout could not be created.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            PolarCheckoutResponse? checkout = await polarResponse.Content.ReadFromJsonAsync<PolarCheckoutResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(checkout?.Url))
            {
                return Results.Problem(
                    "Polar checkout response did not include a checkout URL.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new CheckoutResponse(checkout.Url));
        })
            .WithName("CreateBillingCheckout")
            .WithSummary("Creates a Polar checkout session for the current user.");

        return endpoints;
    }

    private static string NormalizeTier(string tier) => tier.Trim().ToLowerInvariant().Replace('-', '_');

    private static string NormalizeBillingCycle(string billingCycle) => billingCycle.Trim().ToLowerInvariant();

    private static string? GetProductId(PolarProductOptions products, string tier, string billingCycle) => (tier, billingCycle) switch
    {
        (BillingTier.Standard, "monthly") => products.StandardMonthly,
        (BillingTier.Standard, "annual") => products.StandardAnnual,
        (BillingTier.WorldCup, "monthly") => products.WorldCupMonthly,
        (BillingTier.WorldCup, "annual") => products.WorldCupAnnual,
        (BillingTier.Plus, "monthly") => products.PlusMonthly,
        (BillingTier.Plus, "annual") => products.PlusAnnual,
        _ => null
    };

    private sealed record CheckoutRequest(string Tier, string BillingCycle);

    private sealed record CheckoutResponse(string CheckoutUrl);

    private sealed record PolarCheckoutCreate(
        [property: JsonPropertyName("products")] string[] Products,
        [property: JsonPropertyName("success_url")] string SuccessUrl,
        [property: JsonPropertyName("return_url")] string ReturnUrl,
        [property: JsonPropertyName("external_customer_id")] string ExternalCustomerId,
        [property: JsonPropertyName("customer_email")] string CustomerEmail,
        [property: JsonPropertyName("customer_name")] string CustomerName,
        [property: JsonPropertyName("metadata")] Dictionary<string, string> Metadata);

    private sealed record PolarCheckoutResponse([property: JsonPropertyName("url")] string Url);
}
