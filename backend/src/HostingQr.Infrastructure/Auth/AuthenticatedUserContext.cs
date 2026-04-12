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
        return GetCurrentUser().Id;
    }

    public CurrentUser GetCurrentUser()
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        string? rawId = user?.FindFirstValue(AuthConstants.UserIdClaimType);
        string? email = user?.FindFirstValue(ClaimTypes.Email);
        string? displayName = user?.FindFirstValue(ClaimTypes.Name);

        if (rawId is null || !Guid.TryParse(rawId, out Guid userId))
        {
            throw new InvalidOperationException("Authenticated user id is not available in the current request.");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidOperationException("Authenticated user profile is incomplete in the current request.");
        }

        return new CurrentUser(userId, email, displayName);
    }
}
