using HostingQr.Application.Menus;

namespace HostingQr.Application.Abstractions;

public interface IDigitalMenuRepository
{
    Task<DigitalMenuResponse> GetAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task SaveAsync(Guid projectId, SaveDigitalMenuRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateItemAvailabilityAsync(Guid projectId, Guid itemId, bool isOutOfStock, CancellationToken cancellationToken = default);
}
