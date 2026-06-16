namespace HostingQr.Application.Billing;

public sealed record EntitlementResponse(
    string Tier,
    bool IsActive,
    bool GrantedManually,
    DateTimeOffset? EndsAt)
{
    public bool HasToolAccess => IsActive && Tier != BillingTier.None;
}

public static class BillingTier
{
    public const string None = "none";
    public const string Admin = "admin";
    public const string Free = "free";
    public const string Standard = "standard";
    public const string WorldCup = "world_cup";
    public const string Plus = "plus";
}
