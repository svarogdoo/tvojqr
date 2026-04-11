namespace HostingQr.Infrastructure.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";

    public int SessionIdleDays { get; init; } = 14;
}
