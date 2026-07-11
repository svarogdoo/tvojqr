namespace HostingQr.Application.Billing;

public sealed record PlanLimits(string Tier, int MaxProjects, int MaxLanguagesPerProject)
{
    public bool HasUnlimitedProjects => MaxProjects == int.MaxValue;

    public bool HasUnlimitedLanguages => MaxLanguagesPerProject == int.MaxValue;
}

public static class PlanLimitCatalog
{
    public static PlanLimits ForTier(string tier) => tier switch
    {
        BillingTier.Admin => new PlanLimits(BillingTier.Admin, int.MaxValue, int.MaxValue),
        BillingTier.Plus => new PlanLimits(BillingTier.Plus, 5, 7),
        BillingTier.Standard => new PlanLimits(BillingTier.Standard, 1, 3),
        BillingTier.Free => new PlanLimits(BillingTier.Free, 1, 1),
        _ => new PlanLimits(BillingTier.None, 0, 0),
    };
}
