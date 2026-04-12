namespace HostingQr.Infrastructure.Configuration;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public const string PlaceholderClientId = "placeholder-client-id";

    public const string PlaceholderClientSecret = "placeholder-client-secret";

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret)
            && !string.Equals(ClientId, PlaceholderClientId, StringComparison.Ordinal)
            && !string.Equals(ClientSecret, PlaceholderClientSecret, StringComparison.Ordinal);
    }
}
