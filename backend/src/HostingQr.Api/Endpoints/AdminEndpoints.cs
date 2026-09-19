using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using Microsoft.AspNetCore.Authorization;
using HostingQr.Application.Admin;
using HostingQr.Application.Invoices;
using HostingQr.Application.Invitations;

namespace HostingQr.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        group.AddEndpointFilter(async (context, next) =>
        {
            IEntitlementService entitlementService = context.HttpContext.RequestServices.GetRequiredService<IEntitlementService>();
            EntitlementResponse entitlement = await entitlementService.GetCurrentEntitlementAsync(context.HttpContext.RequestAborted);
            return entitlement.IsActive && entitlement.Tier == BillingTier.Admin ? await next(context) : Results.Forbid();
        });

        group.MapGet("/overview", async (
            IEntitlementService entitlementService,
            IAdminOverviewRepository adminOverviewRepository,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await adminOverviewRepository.GetOverviewAsync(cancellationToken));
        })
            .WithName("GetAdminOverview")
            .WithSummary("Returns admin-only platform overview metrics.");

        group.MapGet("/clients", async (IAdminClientRepository repository, CancellationToken cancellationToken) =>
            Results.Ok(await repository.ListClientsAsync(cancellationToken)))
            .WithName("ListAdminClients");

        group.MapGet("/menus", async (IAdminClientRepository repository, CancellationToken cancellationToken) =>
            Results.Ok(await repository.ListMenusAsync(cancellationToken)))
            .WithName("ListAdminMenus");

        group.MapPut("/clients/{userId:guid}/billing", async (Guid userId, UpsertClientBillingRequest request, IAdminClientRepository repository, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.BillingCycle))
            {
                return Results.BadRequest(new { message = "Billing cycle is required." });
            }
            string cycle = request.BillingCycle.Trim().ToLowerInvariant();
            if (cycle is not ("monthly" or "annual"))
            {
                return Results.BadRequest(new { message = "Billing cycle must be monthly or annual." });
            }
            ClientBillingResponse? profile = await repository.UpsertBillingAsync(userId, cycle, request.NextInvoiceDate, request.IsActive, cancellationToken);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).WithName("UpsertClientBilling");

        group.MapGet("/clients/{userId:guid}/invoices", async (Guid userId, InvoiceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(userId, cancellationToken)))
            .WithName("ListClientInvoices");

        group.MapPost("/clients/{userId:guid}/invoices", async (Guid userId, HttpRequest request, InvoiceService service, CancellationToken cancellationToken) =>
        {
            try
            {
                IFormCollection form = await request.ReadFormAsync(cancellationToken);
                if (form.Files.Count != 1 || !DateOnly.TryParse(form["invoiceDate"], out DateOnly invoiceDate))
                {
                    return Results.BadRequest(new { message = "One PDF and a valid invoiceDate are required." });
                }
                return Results.Created("/api/invoices", await service.UploadAsync(userId, invoiceDate, form.Files[0], cancellationToken));
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).DisableAntiforgery().WithName("UploadClientInvoice");

        group.MapDelete("/invoices/{invoiceId:guid}", async (Guid invoiceId, InvoiceService service, CancellationToken cancellationToken) =>
            await service.DeleteAsync(invoiceId, cancellationToken) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteClientInvoice");

        group.MapPost("/projects/{projectId:guid}/invitations", async (Guid projectId, CreateProjectInvitationRequest request, ProjectInvitationService service, CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Accepted(value: await service.CreateAsync(projectId, request, cancellationToken));
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        }).WithName("CreateProjectInvitation");

        group.MapDelete("/invitations/{invitationId:guid}", async (Guid invitationId, ProjectInvitationService service, CancellationToken cancellationToken) =>
            await service.RevokeAsync(invitationId, cancellationToken) ? Results.NoContent() : Results.NotFound())
            .WithName("RevokeProjectInvitation");

        return endpoints;
    }
}
