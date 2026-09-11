using HostingQr.Application.Abstractions;
using HostingQr.Application.Assets;
using HostingQr.Application.Projects;
using HostingQr.Application.Menus;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.AddEndpointFilter(async (context, next) =>
        {
            IEntitlementService entitlementService = context.HttpContext.RequestServices.GetRequiredService<IEntitlementService>();
            bool hasToolAccess = await entitlementService.CurrentUserHasToolAccessAsync(context.HttpContext.RequestAborted);
            if (!hasToolAccess)
            {
                return Results.Json(
                    new { message = "Choose a pricing tier to use project tools." },
                    statusCode: StatusCodes.Status402PaymentRequired);
            }

            return await next(context);
        });

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
                string languageCode = form.TryGetValue("languageCode", out var value) ? value.ToString() : "en";
                var assets = await assetService.UploadImagesAsync(projectId, languageCode, form.Files, cancellationToken);
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

        group.MapPost("/{projectId:guid}/cover-image", async (Guid projectId, HttpRequest request, IAssetService assetService, CancellationToken cancellationToken) =>
        {
            try
            {
                IFormCollection form = await request.ReadFormAsync(cancellationToken);
                if (form.Files.Count != 1)
                {
                    return Results.BadRequest(new { message = "Choose one cover image." });
                }

                return Results.Ok(await assetService.UploadDigitalMenuCoverAsync(projectId, form.Files[0], cancellationToken));
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
            .WithName("UploadDigitalMenuCover")
            .WithSummary("Uploads or replaces the cover image for a Digital Menu project.");

        group.MapDelete("/{projectId:guid}/cover-image", async (Guid projectId, IAssetService assetService, CancellationToken cancellationToken) =>
        {
            bool deleted = await assetService.DeleteDigitalMenuCoverAsync(projectId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteDigitalMenuCover")
            .WithSummary("Removes the cover image from a Digital Menu project.");

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

        group.MapPost("/{projectId:guid}/languages", async (Guid projectId, CreateProjectLanguageRequest request, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse? project = await projectService.AddLanguageAsync(projectId, request, cancellationToken);
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
        .WithName("AddProjectLanguage")
            .WithSummary("Adds a language variant to a project.");

        group.MapPut("/{projectId:guid}/languages/{languageCode}", async (Guid projectId, string languageCode, UpdateProjectLanguageRequest request, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse? project = await projectService.UpdateLanguageAsync(projectId, languageCode, request, cancellationToken);
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
            .WithName("UpdateProjectLanguage")
            .WithSummary("Updates a language variant for a project.");

        group.MapDelete("/{projectId:guid}/languages/{languageCode}", async (Guid projectId, string languageCode, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            try
            {
                ProjectDetailResponse? project = await projectService.DeleteLanguageAsync(projectId, languageCode, cancellationToken);
                return project is null ? Results.NotFound() : Results.Ok(project);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("DeleteProjectLanguage")
            .WithSummary("Removes a non-default language variant from a project.");

        group.MapDelete("/{projectId:guid}", async (Guid projectId, IProjectService projectService, CancellationToken cancellationToken) =>
        {
            bool deleted = await projectService.DeleteProjectAsync(projectId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteProject")
            .WithSummary("Deletes the current project and its dependent data.");

        group.MapGet("/{projectId:guid}/digital-menu", async (Guid projectId, IDigitalMenuService menuService, CancellationToken cancellationToken) =>
        {
            try
            {
                DigitalMenuResponse? menu = await menuService.GetForOwnerAsync(projectId, cancellationToken);
                return menu is null ? Results.NotFound() : Results.Ok(menu);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("GetDigitalMenu")
            .WithSummary("Returns editable structured content for a Digital Menu project.");

        group.MapPut("/{projectId:guid}/digital-menu", async (Guid projectId, SaveDigitalMenuRequest request, IDigitalMenuService menuService, CancellationToken cancellationToken) =>
        {
            try
            {
                DigitalMenuResponse? menu = await menuService.SaveAsync(projectId, request, cancellationToken);
                return menu is null ? Results.NotFound() : Results.Ok(menu);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("SaveDigitalMenu")
            .WithSummary("Transactionally replaces structured Digital Menu content.");

        group.MapPatch("/{projectId:guid}/digital-menu/items/{itemId:guid}/availability", async (Guid projectId, Guid itemId, UpdateDigitalMenuItemAvailabilityRequest request, IDigitalMenuService menuService, CancellationToken cancellationToken) =>
        {
            try
            {
                bool updated = await menuService.UpdateItemAvailabilityAsync(projectId, itemId, request.IsOutOfStock, cancellationToken);
                return updated ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("UpdateDigitalMenuItemAvailability")
            .WithSummary("Quickly marks one Digital Menu item available or out of stock.");

        return endpoints;
    }
}
