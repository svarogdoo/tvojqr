using HostingQr.Domain;

namespace HostingQr.Application.Abstractions;

public interface IBackendInfoService
{
    BackendServiceInfo GetServiceInfo();
}
