using Microsoft.Extensions.DependencyInjection;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Projects;
using HostingQr.Application.Slugs;

namespace HostingQr.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISlugService, SlugService>();

        return services;
    }
}
