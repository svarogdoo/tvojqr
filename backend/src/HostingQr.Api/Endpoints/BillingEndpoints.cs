using HostingQr.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/billing").WithTags("Billing");

        group.MapGet("/entitlement", [Authorize] async (IEntitlementService entitlementService, CancellationToken cancellationToken) =>
            Results.Ok(await entitlementService.GetCurrentEntitlementAsync(cancellationToken)))
            .WithName("GetCurrentEntitlement")
            .WithSummary("Returns the current user's active pricing tier entitlement.");

        return endpoints;
    }
}
