namespace HostingQr.Infrastructure.Configuration;

public sealed class DevelopmentUserOptions
{
    public const string SectionName = "DevelopmentUser";

    public Guid Id { get; init; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public string Email { get; init; } = "demo@hostingqr.local";

    public string DisplayName { get; init; } = "Demo User";
}
