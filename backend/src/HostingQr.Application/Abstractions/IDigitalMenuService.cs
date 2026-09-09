using HostingQr.Application.Menus;

namespace HostingQr.Application.Abstractions;

public interface IDigitalMenuService
{
    Task<DigitalMenuResponse?> GetForOwnerAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<DigitalMenuResponse?> SaveAsync(Guid projectId, SaveDigitalMenuRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateItemAvailabilityAsync(Guid projectId, Guid itemId, bool isOutOfStock, CancellationToken cancellationToken = default);

    Task<DigitalMenuResponse> GetPublicAsync(Guid projectId, string timeZone, CancellationToken cancellationToken = default);
}
