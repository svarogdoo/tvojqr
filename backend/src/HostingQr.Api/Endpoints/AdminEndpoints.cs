using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        group.MapGet("/overview", async (
            IEntitlementService entitlementService,
            IAdminOverviewRepository adminOverviewRepository,
            CancellationToken cancellationToken) =>
        {
            EntitlementResponse entitlement = await entitlementService.GetCurrentEntitlementAsync(cancellationToken);
            if (!entitlement.IsActive || entitlement.Tier != BillingTier.Admin)
            {
                return Results.Forbid();
            }

            return Results.Ok(await adminOverviewRepository.GetOverviewAsync(cancellationToken));
        })
            .WithName("GetAdminOverview")
            .WithSummary("Returns admin-only platform overview metrics.");

        return endpoints;
    }
}
