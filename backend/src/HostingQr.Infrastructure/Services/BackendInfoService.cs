using HostingQr.Application.Abstractions;
using HostingQr.Domain;
using Microsoft.Extensions.Hosting;

namespace HostingQr.Infrastructure.Services;

public sealed class BackendInfoService : IBackendInfoService
{
    private readonly IHostEnvironment _environment;

    public BackendInfoService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public BackendServiceInfo GetServiceInfo()
    {
        return new BackendServiceInfo("HostingQr.Api", _environment.EnvironmentName, DateTimeOffset.UtcNow);
    }
}
