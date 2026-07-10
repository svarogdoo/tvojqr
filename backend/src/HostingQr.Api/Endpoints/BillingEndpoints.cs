using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
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

        group.MapPost("/portal", [Authorize] async (
            ICurrentUserContext currentUserContext,
            IHttpClientFactory httpClientFactory,
            IOptions<PolarOptions> polarOptions,
            IOptions<AuthOptions> authOptions,
            CancellationToken cancellationToken) =>
        {
            PolarOptions options = polarOptions.Value;
            if (string.IsNullOrWhiteSpace(options.AccessToken))
            {
                return Results.Problem(
                    "Polar customer portal is not configured.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            CurrentUser currentUser = currentUserContext.GetCurrentUser();
            string returnUrl = $"{authOptions.Value.FrontendBaseUrl.TrimEnd('/')}/account";
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);

            PolarCustomerSessionCreate polarRequest = new(currentUser.Id.ToString(), returnUrl);
            HttpResponseMessage polarResponse = await httpClient.PostAsJsonAsync("v1/customer-sessions/", polarRequest, cancellationToken);
            if (!polarResponse.IsSuccessStatusCode)
            {
                return Results.Problem(
                    "Polar customer portal could not be opened.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            PolarCustomerSessionResponse? portal = await polarResponse.Content.ReadFromJsonAsync<PolarCustomerSessionResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(portal?.CustomerPortalUrl))
            {
                return Results.Problem(
                    "Polar customer portal response did not include a portal URL.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new BillingPortalResponse(portal.CustomerPortalUrl));
        })
            .WithName("CreateBillingPortalSession")
            .WithSummary("Creates a Polar customer portal session for the current user.");

        group.MapPost("/polar/webhook", async (
            HttpRequest request,
            IEntitlementRepository entitlementRepository,
            IBillingEventRepository billingEventRepository,
            IOptions<PolarOptions> polarOptions,
            CancellationToken cancellationToken) =>
        {
            string webhookSecret = polarOptions.Value.WebhookSecret;
            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                return Results.Problem(
                    "Polar webhook is not configured.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            using StreamReader reader = new(request.Body, Encoding.UTF8);
            string payload = await reader.ReadToEndAsync(cancellationToken);

            if (!VerifyStandardWebhookSignature(request.Headers, payload, webhookSecret))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            string providerEventId = request.Headers["webhook-id"].FirstOrDefault() ?? string.Empty;

            PolarWebhookEvent? webhookEvent = ParsePolarWebhook(payload);
            if (webhookEvent is null)
            {
                await RecordBillingEventAsync(
                    billingEventRepository,
                    providerEventId,
                    ExtractWebhookType(payload),
                    null,
                    "ignored",
                    null,
                    null,
                    payload,
                    cancellationToken);
                return Results.Accepted();
            }

            if (webhookEvent.Action is PolarWebhookAction.Ignore)
            {
                await RecordBillingEventAsync(
                    billingEventRepository,
                    providerEventId,
                    webhookEvent.Type,
                    webhookEvent,
                    "ignored",
                    null,
                    null,
                    payload,
                    cancellationToken);
                return Results.Accepted();
            }

            if (!Guid.TryParse(webhookEvent.UserId, out Guid userId) || !IsCustomerTier(webhookEvent.Tier))
            {
                await RecordBillingEventAsync(
                    billingEventRepository,
                    providerEventId,
                    webhookEvent.Type,
                    webhookEvent,
                    "ignored",
                    null,
                    null,
                    payload,
                    cancellationToken);
                return Results.Accepted();
            }

            if (webhookEvent.Action is PolarWebhookAction.EndAtPeriodEnd && webhookEvent.EndsAt is null)
            {
                await RecordBillingEventAsync(
                    billingEventRepository,
                    providerEventId,
                    webhookEvent.Type,
                    webhookEvent,
                    "ignored",
                    null,
                    null,
                    payload,
                    cancellationToken);
                return Results.Accepted();
            }

            bool isActive = webhookEvent.Action is PolarWebhookAction.Activate
                || (webhookEvent.EndsAt is not null && webhookEvent.EndsAt > DateTimeOffset.UtcNow);

            await entitlementRepository.UpsertAsync(userId, webhookEvent.Tier, isActive, webhookEvent.EndsAt, cancellationToken: cancellationToken);
            await RecordBillingEventAsync(
                billingEventRepository,
                providerEventId,
                webhookEvent.Type,
                webhookEvent,
                GetProcessedAction(webhookEvent.Action),
                isActive,
                webhookEvent.EndsAt,
                payload,
                cancellationToken);
            return Results.Accepted();
        })
            .WithName("HandlePolarWebhook")
            .WithSummary("Handles Polar billing webhooks.");

        return endpoints;
    }

    private static string NormalizeTier(string tier) => tier.Trim().ToLowerInvariant().Replace('-', '_');

    private static string NormalizeBillingCycle(string billingCycle) => billingCycle.Trim().ToLowerInvariant();

    private static string? GetProductId(PolarProductOptions products, string tier, string billingCycle) => (tier, billingCycle) switch
    {
        (BillingTier.Standard, "monthly") => products.StandardMonthly,
        (BillingTier.Standard, "annual") => products.StandardAnnual,
        (BillingTier.Plus, "monthly") => products.PlusMonthly,
        (BillingTier.Plus, "annual") => products.PlusAnnual,
        _ => null
    };

    private static bool IsCustomerTier(string tier) => tier is BillingTier.Standard or BillingTier.Plus;

    private static bool VerifyStandardWebhookSignature(IHeaderDictionary headers, string payload, string secret)
    {
        string? webhookId = headers["webhook-id"].FirstOrDefault();
        string? webhookTimestamp = headers["webhook-timestamp"].FirstOrDefault();
        string? webhookSignature = headers["webhook-signature"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(webhookId)
            || string.IsNullOrWhiteSpace(webhookTimestamp)
            || string.IsNullOrWhiteSpace(webhookSignature))
        {
            return false;
        }

        if (!long.TryParse(webhookTimestamp, out long timestampSeconds))
        {
            return false;
        }

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);
        if (timestamp < DateTimeOffset.UtcNow.AddMinutes(-5) || timestamp > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return false;
        }

        byte[] secretBytes = DecodeWebhookSecret(secret);
        string signedContent = $"{webhookId}.{webhookTimestamp}.{payload}";
        byte[] signatureBytes = HMACSHA256.HashData(secretBytes, Encoding.UTF8.GetBytes(signedContent));
        string expectedSignature = Convert.ToBase64String(signatureBytes);

        foreach (string candidate in webhookSignature.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string signature = candidate.StartsWith("v1,", StringComparison.OrdinalIgnoreCase)
                ? candidate[3..]
                : candidate;

            if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedSignature), Encoding.UTF8.GetBytes(signature)))
            {
                return true;
            }
        }

        return false;
    }

    private static byte[] DecodeWebhookSecret(string secret)
    {
        string normalized = secret.Trim();
        if (normalized.StartsWith("whsec_", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[6..];
        }

        try
        {
            return Convert.FromBase64String(normalized);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(secret);
        }
    }

    private static PolarWebhookEvent? ParsePolarWebhook(string payload)
    {
        using JsonDocument document = JsonDocument.Parse(payload);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("type", out JsonElement typeElement) || typeElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        string eventType = typeElement.GetString() ?? string.Empty;
        PolarWebhookAction action = GetWebhookAction(eventType);
        if (action is PolarWebhookAction.Ignore)
        {
            return new PolarWebhookEvent(eventType, action, string.Empty, string.Empty, null, null, null, null);
        }

        if (!root.TryGetProperty("data", out JsonElement data) || data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        JsonElement? metadata = GetObjectProperty(data, "metadata") ?? GetObjectProperty(data, "customer_metadata");
        string? userId = GetStringProperty(metadata, "userId");
        string? tier = NormalizeWebhookTier(GetStringProperty(metadata, "tier"));
        DateTimeOffset? endsAt = GetDateTimeOffsetProperty(data, "current_period_end")
            ?? GetDateTimeOffsetProperty(data, "ends_at")
            ?? GetDateTimeOffsetProperty(data, "ended_at");
        string? dataId = GetStringProperty(data, "id");
        string? subscriptionId = GetStringProperty(data, "subscription_id")
            ?? (eventType.StartsWith("subscription.", StringComparison.OrdinalIgnoreCase) ? dataId : null);
        string? orderId = GetStringProperty(data, "order_id")
            ?? (eventType.StartsWith("order.", StringComparison.OrdinalIgnoreCase) ? dataId : null);
        string? customerId = GetStringProperty(data, "customer_id")
            ?? GetStringProperty(GetObjectProperty(data, "customer"), "id");

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tier))
        {
            return new PolarWebhookEvent(eventType, action, userId ?? string.Empty, tier ?? string.Empty, endsAt, subscriptionId, orderId, customerId);
        }

        return new PolarWebhookEvent(eventType, action, userId, tier, endsAt, subscriptionId, orderId, customerId);
    }

    private static async Task RecordBillingEventAsync(
        IBillingEventRepository billingEventRepository,
        string providerEventId,
        string eventType,
        PolarWebhookEvent? webhookEvent,
        string processedAction,
        bool? entitlementActive,
        DateTimeOffset? entitlementEndsAt,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        Guid? userId = Guid.TryParse(webhookEvent?.UserId, out Guid parsedUserId) ? parsedUserId : null;
        string? tier = string.IsNullOrWhiteSpace(webhookEvent?.Tier) ? null : webhookEvent.Tier;

        await billingEventRepository.InsertAsync(new BillingEventRecord(
            "polar",
            providerEventId,
            eventType,
            userId,
            tier,
            webhookEvent?.SubscriptionId,
            webhookEvent?.OrderId,
            webhookEvent?.CustomerId,
            processedAction,
            entitlementActive,
            entitlementEndsAt,
            rawPayload), cancellationToken);
    }

    private static string GetProcessedAction(PolarWebhookAction action) => action switch
    {
        PolarWebhookAction.Activate => "activate",
        PolarWebhookAction.EndAtPeriodEnd => "end_at_period_end",
        PolarWebhookAction.Deactivate => "deactivate",
        _ => "ignored",
    };

    private static string ExtractWebhookType(string payload)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(payload);
            JsonElement root = document.RootElement;
            return root.TryGetProperty("type", out JsonElement typeElement) && typeElement.ValueKind == JsonValueKind.String
                ? typeElement.GetString() ?? "unknown"
                : "unknown";
        }
        catch (JsonException)
        {
            return "unknown";
        }
    }

    private static PolarWebhookAction GetWebhookAction(string eventType) => eventType switch
    {
        "order.paid" => PolarWebhookAction.Activate,
        "subscription.active" => PolarWebhookAction.Activate,
        "subscription.created" => PolarWebhookAction.Activate,
        "subscription.updated" => PolarWebhookAction.Activate,
        "subscription.canceled" => PolarWebhookAction.EndAtPeriodEnd,
        "subscription.revoked" => PolarWebhookAction.Deactivate,
        _ => PolarWebhookAction.Ignore,
    };

    private static JsonElement? GetObjectProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Object
            ? property
            : null;
    }

    private static string? GetStringProperty(JsonElement? element, string propertyName)
    {
        if (element is null || !element.Value.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind is JsonValueKind.String ? property.GetString() : null;
    }

    private static DateTimeOffset? GetDateTimeOffsetProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return DateTimeOffset.TryParse(property.GetString(), out DateTimeOffset value) ? value : null;
    }

    private static string? NormalizeWebhookTier(string? tier) => string.IsNullOrWhiteSpace(tier)
        ? null
        : NormalizeTier(tier);

    private sealed record CheckoutRequest(string Tier, string BillingCycle);

    private sealed record CheckoutResponse(string CheckoutUrl);

    private sealed record BillingPortalResponse(string PortalUrl);

    private sealed record PolarCheckoutCreate(
        [property: JsonPropertyName("products")] string[] Products,
        [property: JsonPropertyName("success_url")] string SuccessUrl,
        [property: JsonPropertyName("return_url")] string ReturnUrl,
        [property: JsonPropertyName("external_customer_id")] string ExternalCustomerId,
        [property: JsonPropertyName("customer_email")] string CustomerEmail,
        [property: JsonPropertyName("customer_name")] string CustomerName,
        [property: JsonPropertyName("metadata")] Dictionary<string, string> Metadata);

    private sealed record PolarCheckoutResponse([property: JsonPropertyName("url")] string Url);

    private sealed record PolarCustomerSessionCreate(
        [property: JsonPropertyName("external_customer_id")] string ExternalCustomerId,
        [property: JsonPropertyName("return_url")] string ReturnUrl);

    private sealed record PolarCustomerSessionResponse([property: JsonPropertyName("customer_portal_url")] string CustomerPortalUrl);

    private sealed record PolarWebhookEvent(
        string Type,
        PolarWebhookAction Action,
        string UserId,
        string Tier,
        DateTimeOffset? EndsAt,
        string? SubscriptionId,
        string? OrderId,
        string? CustomerId);

    private enum PolarWebhookAction
    {
        Ignore,
        Activate,
        EndAtPeriodEnd,
        Deactivate,
    }
}
