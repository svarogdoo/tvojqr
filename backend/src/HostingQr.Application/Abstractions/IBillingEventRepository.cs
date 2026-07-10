using HostingQr.Application.Billing;

namespace HostingQr.Application.Abstractions;

public interface IBillingEventRepository
{
    Task InsertAsync(BillingEventRecord billingEvent, CancellationToken cancellationToken = default);
}
