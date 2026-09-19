using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Admin;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Admin;

public sealed class AdminClientRepository : IAdminClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AdminClientRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<AdminClientResponse>> ListClientsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select u.id as UserId, u.email, u.display_name as DisplayName, u.created_at as JoinedAt,
                   p.menu_type as MenuType, bp.billing_cycle as BillingCycle, bp.is_active as BillingActive,
                   bp.next_invoice_date as NextInvoiceDate,
                   (select max(i.invoice_date) from invoices i where i.client_user_id = u.id) as LastInvoiceDate
            from users u
            left join projects p on p.owner_user_id = u.id
            left join client_billing_profiles bp on bp.user_id = u.id
            where not exists (
                select 1 from user_entitlements e
                where e.user_id = u.id and e.tier = 'admin' and e.is_active = true and (e.ends_at is null or e.ends_at > now())
            )
            order by u.created_at desc, p.menu_type;
            """;
        using var connection = _connectionFactory.CreateConnection();
        ClientRow[] rows = (await connection.QueryAsync<ClientRow>(new CommandDefinition(sql, cancellationToken: cancellationToken))).ToArray();
        return rows.GroupBy(row => row.UserId).Select(group =>
        {
            ClientRow first = group.First();
            Dictionary<string, int> types = group.Where(row => row.MenuType is not null)
                .GroupBy(row => row.MenuType!)
                .ToDictionary(type => type.Key, type => type.Count());
            return new AdminClientResponse(first.UserId, first.Email, first.DisplayName, first.JoinedAt,
                types.Values.Sum(), types, first.BillingCycle, first.BillingActive, first.LastInvoiceDate, first.NextInvoiceDate);
        }).ToArray();
    }

    public async Task<IReadOnlyList<AdminMenuResponse>> ListMenusAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select p.id as ProjectId, p.name, s.slug, p.status, p.menu_type as MenuType, p.updated_at as UpdatedAt,
                   u.id as ClientUserId, u.email as ClientEmail, u.display_name as ClientDisplayName
            from projects p
            inner join users u on u.id = p.owner_user_id
            inner join slugs s on s.project_id = p.id and s.is_primary = true
            order by p.updated_at desc;
            """;
        using var connection = _connectionFactory.CreateConnection();
        return (await connection.QueryAsync<AdminMenuResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken))).ToArray();
    }

    public async Task<AccountSummaryResponse?> GetAccountSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select u.created_at as JoinedAt, p.menu_type as MenuType,
                   bp.billing_cycle as BillingCycle, bp.is_active as BillingActive,
                   bp.next_invoice_date as NextInvoiceDate,
                   (select max(i.invoice_date) from invoices i where i.client_user_id = u.id) as LastInvoiceDate
            from users u
            left join projects p on p.owner_user_id = u.id
            left join client_billing_profiles bp on bp.user_id = u.id
            where u.id = @UserId
            order by p.menu_type;
            """;
        using var connection = _connectionFactory.CreateConnection();
        AccountSummaryRow[] rows = (await connection.QueryAsync<AccountSummaryRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken))).ToArray();
        if (rows.Length == 0)
        {
            return null;
        }

        AccountSummaryRow first = rows[0];
        Dictionary<string, int> types = rows.Where(row => row.MenuType is not null)
            .GroupBy(row => row.MenuType!)
            .ToDictionary(type => type.Key, type => type.Count());
        return new AccountSummaryResponse(first.JoinedAt, types.Values.Sum(), types, first.BillingCycle,
            first.BillingActive, first.LastInvoiceDate, first.NextInvoiceDate);
    }

    public async Task<ClientBillingResponse?> GetBillingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select user_id as UserId, billing_cycle as BillingCycle, next_invoice_date as NextInvoiceDate,
                is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            from client_billing_profiles where user_id = @UserId;
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ClientBillingResponse>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<ClientBillingResponse?> UpsertBillingAsync(Guid userId, string billingCycle, DateOnly? nextInvoiceDate, bool isActive, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into client_billing_profiles (user_id, billing_cycle, next_invoice_date, is_active)
            select id, @BillingCycle, @NextInvoiceDate, @IsActive from users where id = @UserId
            on conflict (user_id) do update set billing_cycle = excluded.billing_cycle,
                next_invoice_date = excluded.next_invoice_date, is_active = excluded.is_active, updated_at = now()
            returning user_id as UserId, billing_cycle as BillingCycle, next_invoice_date as NextInvoiceDate,
                is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt;
            """;
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var parameters = new { UserId = userId, BillingCycle = billingCycle, NextInvoiceDate = nextInvoiceDate, IsActive = isActive };
        ClientBillingResponse? profile = await connection.QuerySingleOrDefaultAsync<ClientBillingResponse>(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        if (profile is null)
        {
            transaction.Rollback();
            return null;
        }

        const string entitlementSql = """
            insert into user_entitlements (user_id, tier, is_active, granted_manually, starts_at, ends_at, updated_at)
            values (@UserId, 'plus', @IsActive, true, now(), null, now())
            on conflict (user_id) do update set
                tier = case when user_entitlements.tier = 'admin' and user_entitlements.is_active then 'admin' else 'plus' end,
                is_active = case when user_entitlements.tier = 'admin' and user_entitlements.is_active then true else @IsActive end,
                granted_manually = case when user_entitlements.tier = 'admin' and user_entitlements.is_active then user_entitlements.granted_manually else true end,
                ends_at = case when user_entitlements.tier = 'admin' and user_entitlements.is_active then user_entitlements.ends_at else null end,
                updated_at = now();
            """;
        await connection.ExecuteAsync(new CommandDefinition(entitlementSql, parameters, transaction, cancellationToken: cancellationToken));
        transaction.Commit();
        return profile;
    }

    private sealed record ClientRow(Guid UserId, string Email, string DisplayName, DateTimeOffset JoinedAt, string? MenuType,
        string? BillingCycle, bool? BillingActive, DateOnly? LastInvoiceDate, DateOnly? NextInvoiceDate);

    private sealed record AccountSummaryRow(DateTimeOffset JoinedAt, string? MenuType, string? BillingCycle,
        bool? BillingActive, DateOnly? LastInvoiceDate, DateOnly? NextInvoiceDate);
}
