using System.Security.Claims;
using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HostingQr.Infrastructure.Auth;

public sealed class AuthenticatedUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        string? raw = user?.FindFirstValue(AuthConstants.UserIdClaimType);
        if (raw is null || !Guid.TryParse(raw, out Guid userId))
        {
            throw new InvalidOperationException("Authenticated user id is not available in the current request.");
        }

        return userId;
    }
}
