using System.Security.Claims;
using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Auth;
using HostingQr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HostingQr.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/google", (HttpContext httpContext, IOptions<AuthOptions> authOptions) =>
        {
            if (httpContext.RequestServices.GetRequiredService<IOptions<GoogleAuthOptions>>().Value.ClientId is null or "")
            {
                return Results.Problem("Google auth is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = $"{authOptions.Value.FrontendBaseUrl.TrimEnd('/')}/dashboard"
            };

            return Results.Challenge(properties, [AuthConstants.GoogleScheme]);
        })
            .WithName("StartGoogleAuth")
            .WithSummary("Starts Google sign-in.");

        group.MapGet("/me", [Authorize] async (ClaimsPrincipal user, IUserRepository userRepository, CancellationToken cancellationToken) =>
        {
            string? rawUserId = user.FindFirstValue(AuthConstants.UserIdClaimType);
            if (rawUserId is null || !Guid.TryParse(rawUserId, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var currentUser = await userRepository.GetByIdAsync(userId, cancellationToken);
            return currentUser is null ? Results.Unauthorized() : Results.Ok(currentUser);
        })
            .WithName("GetCurrentUser")
            .WithSummary("Returns the current authenticated user.");

        group.MapPost("/sign-out", [Authorize] async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(AuthConstants.CookieScheme);
            return Results.NoContent();
        })
            .WithName("SignOut")
            .WithSummary("Signs out the current user.");

        return endpoints;
    }
}
