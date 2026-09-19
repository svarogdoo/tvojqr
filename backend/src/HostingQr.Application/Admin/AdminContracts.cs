namespace HostingQr.Application.Admin;

public sealed record AdminClientResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTimeOffset JoinedAt,
    int MenuCount,
    IReadOnlyDictionary<string, int> MenuTypes,
    string? BillingCycle,
    bool? BillingActive,
    DateOnly? LastInvoiceDate,
    DateOnly? NextInvoiceDate);

public sealed record AdminMenuResponse(
    Guid ProjectId,
    string Name,
    string Slug,
    string Status,
    string MenuType,
    DateTimeOffset UpdatedAt,
    Guid ClientUserId,
    string ClientEmail,
    string ClientDisplayName);

public sealed record UpsertClientBillingRequest(string BillingCycle, DateOnly? NextInvoiceDate, bool IsActive);

public sealed record ClientBillingResponse(
    Guid UserId,
    string BillingCycle,
    DateOnly? NextInvoiceDate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AccountSummaryResponse(
    DateTimeOffset JoinedAt,
    int MenuCount,
    IReadOnlyDictionary<string, int> MenuTypes,
    string? BillingCycle,
    bool? BillingActive,
    DateOnly? LastInvoiceDate,
    DateOnly? NextInvoiceDate);
