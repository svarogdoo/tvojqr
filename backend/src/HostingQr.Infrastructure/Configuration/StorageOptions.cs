namespace HostingQr.Infrastructure.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; init; } = string.Empty;

    public string? UploadsRootPath { get; init; }

    public R2StorageOptions R2 { get; init; } = new();

    public bool UsesR2() => string.Equals(Provider, "R2", StringComparison.OrdinalIgnoreCase) ||
        (string.IsNullOrWhiteSpace(Provider) && R2.IsConfigured());
}

public sealed class R2StorageOptions
{
    public string? AccountId { get; init; }

    public string? AccessKeyId { get; init; }

    public string? SecretAccessKey { get; init; }

    public string? BucketName { get; init; }

    public string? PublicBaseUrl { get; init; }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(AccountId)
            && !string.IsNullOrWhiteSpace(AccessKeyId)
            && !string.IsNullOrWhiteSpace(SecretAccessKey)
            && !string.IsNullOrWhiteSpace(BucketName)
            && !string.IsNullOrWhiteSpace(PublicBaseUrl);
    }
}
