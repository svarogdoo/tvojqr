using HostingQr.Application.Abstractions;

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

        return endpoints;
    }
}
