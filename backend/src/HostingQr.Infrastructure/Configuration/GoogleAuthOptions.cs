namespace HostingQr.Infrastructure.Configuration;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }
}
