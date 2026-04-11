using HostingQr.Application.Abstractions;

namespace HostingQr.Api.Endpoints;

public static class SlugEndpoints
{
    public static IEndpointRouteBuilder MapSlugEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/slugs").WithTags("Slugs");

        group.MapGet("/{slug}/availability", async (string slug, ISlugService slugService, CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await slugService.CheckAvailabilityAsync(slug, cancellationToken));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
            .WithName("CheckSlugAvailability")
            .WithSummary("Checks whether a custom slug is available.");

        group.MapPost("/generate", async (ISlugService slugService, CancellationToken cancellationToken) =>
            Results.Ok(await slugService.GenerateUniqueSlugAsync(cancellationToken)))
            .WithName("GenerateSlug")
            .WithSummary("Generates a random unique slug.");

        return endpoints;
    }
}
