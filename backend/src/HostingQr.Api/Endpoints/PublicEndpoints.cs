using HostingQr.Application.Abstractions;
using HostingQr.Domain.Projects;

namespace HostingQr.Api.Endpoints;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/public").WithTags("Public");

        group.MapGet("/{slug}", async (string slug, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await projectService.GetPublicProjectAsync(slug, cancellationToken);
                if (project is null)
                {
                    return Results.NotFound();
                }

                if (project.Status == HostingQr.Domain.Projects.ProjectStatus.Disabled)
                {
                    return Results.Json(project, statusCode: StatusCodes.Status410Gone);
                }

                return Results.Ok(project);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("GetPublicProjectBySlug")
            .WithSummary("Looks up a public project by slug.");

        group.MapGet("/{slug}/digital-menu", async (string slug, ISlugService slugService, IProjectRepository projectRepository, IDigitalMenuService menuService, CancellationToken cancellationToken) =>
        {
            try
            {
                string normalizedSlug = slugService.NormalizeOrThrow(slug);
                var project = await projectRepository.GetPublicBySlugAsync(normalizedSlug, cancellationToken);
                if (project is null || project.Status != ProjectStatus.Active || project.MenuType != ProjectMenuType.Digital)
                {
                    return Results.NotFound();
                }

                return Results.Ok(await menuService.GetPublicAsync(project.ProjectId, project.TimeZone, cancellationToken));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("GetPublicDigitalMenu")
            .WithSummary("Returns currently visible structured content for a public Digital Menu.");

        return endpoints;
    }
}
