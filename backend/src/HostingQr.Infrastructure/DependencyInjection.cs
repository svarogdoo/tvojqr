using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using HostingQr.Infrastructure.Data;
using HostingQr.Infrastructure.Migrations;
using HostingQr.Infrastructure.Projects;
using HostingQr.Infrastructure.Services;
using HostingQr.Infrastructure.Slugs;
using HostingQr.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<IBackendInfoService, BackendInfoService>();
        services.AddScoped<ICurrentUserContext, DevelopmentUserContext>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ISlugRepository, SlugRepository>();
        services.AddSingleton<MigrationRunner>();

        return services;
    }
}
