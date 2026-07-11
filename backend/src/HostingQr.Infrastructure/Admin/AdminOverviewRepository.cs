using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Admin;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Admin;

public sealed class AdminOverviewRepository : IAdminOverviewRepository
{
    private static readonly string[] KnownTiers =
    [
        BillingTier.None,
        BillingTier.Admin,
        BillingTier.Free,
        BillingTier.Standard,
        BillingTier.Plus,
    ];

    private readonly IDbConnectionFactory _connectionFactory;

    public AdminOverviewRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        const string totalAccountsSql = "select count(*) from users;";
        const string totalViewsSql = "select coalesce(sum(total_views), 0) from project_view_counts;";
        const string tiersSql = """
            select
                coalesce(e.tier, 'none') as Tier,
                count(*) as Count
            from users u
            left join user_entitlements e on e.user_id = u.id
                and e.is_active = true
                and (e.ends_at is null or e.ends_at > now())
            group by coalesce(e.tier, 'none');
            """;

        using var connection = _connectionFactory.CreateConnection();
        long totalAccounts = await connection.ExecuteScalarAsync<long>(new CommandDefinition(totalAccountsSql, cancellationToken: cancellationToken));
        long totalViews = await connection.ExecuteScalarAsync<long>(new CommandDefinition(totalViewsSql, cancellationToken: cancellationToken));
        var tierRows = await connection.QueryAsync<TierCountRow>(new CommandDefinition(tiersSql, cancellationToken: cancellationToken));

        Dictionary<string, long> accountsByTier = KnownTiers.ToDictionary(tier => tier, _ => 0L);
        foreach (TierCountRow row in tierRows)
        {
            accountsByTier[row.Tier] = row.Count;
        }

        return new AdminOverviewResponse(totalAccounts, totalViews, accountsByTier);
    }

    private sealed class TierCountRow
    {
        public string Tier { get; init; } = BillingTier.None;

        public long Count { get; init; }
    }
}
