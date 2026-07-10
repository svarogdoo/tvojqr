using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Billing;

public sealed class BillingEventRepository : IBillingEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BillingEventRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(BillingEventRecord billingEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into billing_events (
                provider,
                provider_event_id,
                event_type,
                user_id,
                tier,
                subscription_id,
                order_id,
                customer_id,
                processed_action,
                entitlement_active,
                entitlement_ends_at,
                raw_payload
            )
            values (
                @Provider,
                @ProviderEventId,
                @EventType,
                @UserId,
                @Tier,
                @SubscriptionId,
                @OrderId,
                @CustomerId,
                @ProcessedAction,
                @EntitlementActive,
                @EntitlementEndsAt,
                cast(@RawPayload as jsonb)
            )
            on conflict (provider, provider_event_id) do nothing;
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            billingEvent.Provider,
            billingEvent.ProviderEventId,
            billingEvent.EventType,
            billingEvent.UserId,
            billingEvent.Tier,
            billingEvent.SubscriptionId,
            billingEvent.OrderId,
            billingEvent.CustomerId,
            billingEvent.ProcessedAction,
            billingEvent.EntitlementActive,
            EntitlementEndsAt = billingEvent.EntitlementEndsAt?.UtcDateTime,
            billingEvent.RawPayload,
        }, cancellationToken: cancellationToken));
    }
}
