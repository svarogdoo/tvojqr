namespace HostingQr.Infrastructure.Configuration;

public sealed class PrivateInvoiceStorageOptions
{
    public const string SectionName = "PrivateInvoices";
    public string Provider { get; init; } = "Local";
    public string? RootPath { get; init; }
    public PrivateR2Options R2 { get; init; } = new();
    public bool UsesR2() => string.Equals(Provider, "R2", StringComparison.OrdinalIgnoreCase);
}

public sealed class PrivateR2Options
{
    public string? AccountId { get; init; }
    public string? AccessKeyId { get; init; }
    public string? SecretAccessKey { get; init; }
    public string? BucketName { get; init; }

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(AccountId) && !string.IsNullOrWhiteSpace(AccessKeyId) &&
        !string.IsNullOrWhiteSpace(SecretAccessKey) && !string.IsNullOrWhiteSpace(BucketName);
}
