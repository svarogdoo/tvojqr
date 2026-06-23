using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using HostingQr.Infrastructure.Auth;
using HostingQr.Infrastructure.Assets;
using HostingQr.Infrastructure.Billing;
using HostingQr.Infrastructure.Configuration;
using HostingQr.Infrastructure.Data;
using HostingQr.Infrastructure.Migrations;
using HostingQr.Infrastructure.Projects;
using HostingQr.Infrastructure.Services;
using HostingQr.Infrastructure.Slugs;
using HostingQr.Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName));

        services
            .AddOptions<DevelopmentUserOptions>()
            .Bind(configuration.GetSection(DevelopmentUserOptions.SectionName));

        services
            .AddOptions<MigrationOptions>()
            .Bind(configuration.GetSection(MigrationOptions.SectionName));

        services
            .AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName));

        services
            .AddOptions<GoogleAuthOptions>()
            .Bind(configuration.GetSection(GoogleAuthOptions.SectionName));

        services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName));

        services
            .AddOptions<PolarOptions>()
            .Bind(configuration.GetSection(PolarOptions.SectionName));

        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddCors(options =>
        {
            AuthOptions authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
            options.AddPolicy("FrontendClient", policy =>
            {
                policy
                    .WithOrigins(authOptions.FrontendBaseUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = AuthConstants.CookieScheme;
                options.DefaultAuthenticateScheme = AuthConstants.CookieScheme;
                options.DefaultChallengeScheme = AuthConstants.CookieScheme;
            })
            .AddCookie(AuthConstants.CookieScheme, options =>
            {
                AuthOptions authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
                options.LoginPath = "/api/auth/google";
                options.LogoutPath = "/api/auth/sign-out";
                options.Cookie.Name = "hostingqr.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(authOptions.SessionIdleDays);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddGoogle(AuthConstants.GoogleScheme, _ => { });

        services.AddAuthorization();
        services.AddSingleton<IConfigureOptions<GoogleOptions>, GoogleAuthenticationConfigurator>();

        StorageOptions storageOptions = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<IBackendInfoService, BackendInfoService>();
        services.AddSingleton<IAssetStorageService>(provider => storageOptions.UsesR2()
            ? provider.GetRequiredService<R2AssetStorageService>()
            : provider.GetRequiredService<LocalAssetStorageService>());
        services.AddSingleton<LocalAssetStorageService>();
        services.AddSingleton<R2AssetStorageService>();
        services.AddScoped<ICurrentUserContext, AuthenticatedUserContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEntitlementRepository, EntitlementRepository>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectLanguageVariantRepository, ProjectLanguageVariantRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ISlugRepository, SlugRepository>();
        services.AddSingleton<MigrationRunner>();

        return services;
    }
}
