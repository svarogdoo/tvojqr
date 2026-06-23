namespace HostingQr.Infrastructure.Configuration;

public sealed class PolarOptions
{
    public const string SectionName = "Polar";

    public string AccessToken { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.polar.sh";

    public string SuccessUrl { get; set; } = string.Empty;

    public string CancelUrl { get; set; } = string.Empty;

    public PolarProductOptions Products { get; set; } = new();

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(AccessToken)
        && !string.IsNullOrWhiteSpace(SuccessUrl)
        && !string.IsNullOrWhiteSpace(CancelUrl)
        && Products.IsConfigured();
}

public sealed class PolarProductOptions
{
    public string StandardMonthly { get; set; } = string.Empty;

    public string StandardAnnual { get; set; } = string.Empty;

    public string WorldCupMonthly { get; set; } = string.Empty;

    public string WorldCupAnnual { get; set; } = string.Empty;

    public string PlusMonthly { get; set; } = string.Empty;

    public string PlusAnnual { get; set; } = string.Empty;

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(StandardMonthly)
        && !string.IsNullOrWhiteSpace(StandardAnnual)
        && !string.IsNullOrWhiteSpace(WorldCupMonthly)
        && !string.IsNullOrWhiteSpace(WorldCupAnnual)
        && !string.IsNullOrWhiteSpace(PlusMonthly)
        && !string.IsNullOrWhiteSpace(PlusAnnual);
}
