using HostingQr.Application.Admin;

namespace HostingQr.Application.Abstractions;

public interface IAdminOverviewRepository
{
    Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
}
