namespace HostingQr.Infrastructure.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? FromAddress { get; init; }
    public string FromName { get; init; } = "HostingQr";
    public bool EnableSsl { get; init; } = true;

    public bool IsConfigured() => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromAddress);
}
