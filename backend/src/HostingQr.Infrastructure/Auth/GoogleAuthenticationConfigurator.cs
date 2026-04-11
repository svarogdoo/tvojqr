using System.Security.Claims;
using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Auth;

public sealed class GoogleAuthenticationConfigurator : IConfigureNamedOptions<GoogleOptions>
{
    private readonly GoogleAuthOptions _googleOptions;

    public GoogleAuthenticationConfigurator(IOptions<GoogleAuthOptions> googleOptions)
    {
        _googleOptions = googleOptions.Value;
    }

    public void Configure(string? name, GoogleOptions options)
    {
        if (name is not null && name != AuthConstants.GoogleScheme)
        {
            return;
        }

        options.ClientId = _googleOptions.ClientId ?? string.Empty;
        options.ClientSecret = _googleOptions.ClientSecret ?? string.Empty;
        options.SignInScheme = AuthConstants.CookieScheme;
        options.CallbackPath = "/api/auth/google/callback";
        options.SaveTokens = false;

        options.Events.OnCreatingTicket = async context =>
        {
            string? email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Google authentication did not provide an email address.");
            }

            string displayName = context.Identity?.FindFirst(ClaimTypes.Name)?.Value ?? email;
            IUserRepository userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            Guid userId = Guid.NewGuid();
            Claim? existingIdClaim = context.Identity?.FindFirst(AuthConstants.UserIdClaimType);
            if (existingIdClaim is not null && Guid.TryParse(existingIdClaim.Value, out Guid parsed))
            {
                userId = parsed;
            }

            var persistedUser = await userRepository.UpsertAsync(userId, email, displayName, context.HttpContext.RequestAborted);

            context.Identity?.AddClaim(new Claim(AuthConstants.UserIdClaimType, persistedUser.Id.ToString()));
            context.Identity?.AddClaim(new Claim(ClaimTypes.Name, persistedUser.DisplayName));
            context.Identity?.AddClaim(new Claim(ClaimTypes.Email, persistedUser.Email));
        };
    }

    public void Configure(GoogleOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}
