using HostingQr.Application.Admin;

namespace HostingQr.Application.Abstractions;

public interface IAdminClientRepository
{
    Task<IReadOnlyList<AdminClientResponse>> ListClientsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminMenuResponse>> ListMenusAsync(CancellationToken cancellationToken = default);
    Task<AccountSummaryResponse?> GetAccountSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ClientBillingResponse?> GetBillingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ClientBillingResponse?> UpsertBillingAsync(Guid userId, string billingCycle, DateOnly? nextInvoiceDate, bool isActive, CancellationToken cancellationToken = default);
}
