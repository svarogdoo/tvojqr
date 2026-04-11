namespace HostingQr.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid GetCurrentUserId();
}
