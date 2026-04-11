using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Users;

public sealed class DevelopmentUserContext : ICurrentUserContext
{
    private readonly DevelopmentUserOptions _options;

    public DevelopmentUserContext(IOptions<DevelopmentUserOptions> options)
    {
        _options = options.Value;
    }

    public Guid GetCurrentUserId() => _options.Id;
}
