using Microsoft.Extensions.DependencyInjection;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
using HostingQr.Application.Projects;
using HostingQr.Application.Menus;
using HostingQr.Application.Slugs;
using HostingQr.Application.Invoices;
using HostingQr.Application.Invitations;

namespace HostingQr.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IDigitalMenuService, DigitalMenuService>();
        services.AddScoped<IProjectAccessService, ProjectAccessService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<ProjectInvitationService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
