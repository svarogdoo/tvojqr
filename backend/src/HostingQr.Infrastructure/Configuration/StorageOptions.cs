namespace HostingQr.Infrastructure.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string? UploadsRootPath { get; init; }
}
