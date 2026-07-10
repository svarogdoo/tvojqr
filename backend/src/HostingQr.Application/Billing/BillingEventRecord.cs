namespace HostingQr.Application.Billing;

public sealed record BillingEventRecord(
    string Provider,
    string ProviderEventId,
    string EventType,
    Guid? UserId,
    string? Tier,
    string? SubscriptionId,
    string? OrderId,
    string? CustomerId,
    string ProcessedAction,
    bool? EntitlementActive,
    DateTimeOffset? EntitlementEndsAt,
    string RawPayload);
