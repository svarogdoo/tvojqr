namespace HostingQr.Application.Admin;

public sealed record AdminOverviewResponse(
    long TotalAccounts,
    long TotalViews,
    IReadOnlyDictionary<string, long> AccountsByTier);
