using HostingQr.Application.Abstractions;
using HostingQr.Application.Projects;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.MapGet("/", async (IProjectService projectService, CancellationToken cancellationToken) =>
            Results.Ok(await projectService.ListProjectsAsync(cancellationToken)))
            .WithName("ListProjects")
            .WithSummary("Lists projects for the current user.");

        group.MapGet("/{projectId:guid}", async (Guid projectId, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            ProjectDetailResponse? project = await projectService.GetProjectAsync(projectId, cancellationToken);
            return project is null ? Results.NotFound() : Results.Ok(project);
        })
            .WithName("GetProject")
            .WithSummary("Returns project settings for one project.");

        group.MapPost("/", async (CreateProjectRequest request, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse project = await projectService.CreateProjectAsync(request, cancellationToken);
                return Results.Created($"/api/projects/{project.Id}", project);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
            .WithName("CreateProject")
            .WithSummary("Creates a new project with one active slug.");

        return endpoints;
    }
}
