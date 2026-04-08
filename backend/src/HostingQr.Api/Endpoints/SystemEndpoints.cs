using HostingQr.Application.Abstractions;

namespace HostingQr.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api").WithTags("System");

        group.MapGet("/ping", (IBackendInfoService infoService) =>
            Results.Ok(infoService.GetServiceInfo()))
            .WithName("Ping")
            .WithSummary("Returns a simple backend health payload.")
            .WithDescription("Useful as a first smoke test endpoint while the backend foundation is being built.")
            .Produces(StatusCodes.Status200OK);

        return endpoints;
    }
}
