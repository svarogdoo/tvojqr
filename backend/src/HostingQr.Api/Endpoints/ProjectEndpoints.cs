using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
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

        group.MapPut("/{projectId:guid}", async (Guid projectId, UpdateProjectRequest request, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse? project = await projectService.UpdateProjectAsync(projectId, request, cancellationToken);
                return project is null ? Results.NotFound() : Results.Ok(project);
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
            .WithName("UpdateProject")
            .WithSummary("Updates one project name and active slug.");

        group.MapPost("/{projectId:guid}/assets", async (Guid projectId, HttpRequest request, IAssetService assetService, CancellationToken cancellationToken) =>
        {
            try
            {
                IFormCollection form = await request.ReadFormAsync(cancellationToken);
                var assets = await assetService.UploadImagesAsync(projectId, form.Files, cancellationToken);
                return Results.Ok(assets);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
            .DisableAntiforgery()
            .WithName("UploadProjectAssets")
            .WithSummary("Uploads one or more image assets to a project.");

        group.MapDelete("/{projectId:guid}/assets/{assetId:guid}", async (Guid projectId, Guid assetId, IAssetService assetService, CancellationToken cancellationToken) =>
        {
            bool deleted = await assetService.DeleteImageAsync(projectId, assetId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteProjectAsset")
            .WithSummary("Deletes one uploaded image asset from a project.");

        group.MapPut("/{projectId:guid}/assets/order", async (Guid projectId, ReorderAssetsRequest request, IAssetService assetService, CancellationToken cancellationToken) =>
        {
            try
            {
                IReadOnlyList<AssetResponse>? assets = await assetService.ReorderImagesAsync(projectId, request.AssetIds, cancellationToken);
                return assets is null ? Results.NotFound() : Results.Ok(assets);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("ReorderProjectAssets")
            .WithSummary("Updates the display order for project image assets.");

        group.MapPatch("/{projectId:guid}/status", async (Guid projectId, UpdateProjectStatusRequest request, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse? project = await projectService.UpdateProjectStatusAsync(projectId, request, cancellationToken);
                return project is null ? Results.NotFound() : Results.Ok(project);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("UpdateProjectStatus")
            .WithSummary("Updates the current project status.");

        group.MapDelete("/{projectId:guid}", async (Guid projectId, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            bool deleted = await projectService.DeleteProjectAsync(projectId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteProject")
            .WithSummary("Deletes the current project and its dependent data.");

        return endpoints;
    }
}
