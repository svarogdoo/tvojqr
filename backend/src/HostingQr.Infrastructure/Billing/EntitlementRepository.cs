using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Billing;

public sealed class EntitlementRepository : IEntitlementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EntitlementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EntitlementResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                tier as Tier,
                is_active as IsActive,
                granted_manually as GrantedManually,
                ends_at as EndsAt
            from user_entitlements
            where user_id = @UserId
                and is_active = true
                and (ends_at is null or ends_at > now())
            order by updated_at desc
            limit 1;
            """;

        using var connection = _connectionFactory.CreateConnection();
        EntitlementResponse? entitlement = await connection.QuerySingleOrDefaultAsync<EntitlementResponse>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return entitlement ?? new EntitlementResponse(BillingTier.None, false, false, null);
    }
}
