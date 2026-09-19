using HostingQr.Application.Invoices;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/invoices").WithTags("Invoices").RequireAuthorization();

        group.MapGet("/", async (InvoiceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(null, cancellationToken)))
            .WithName("ListOwnInvoices");

        group.MapGet("/{invoiceId:guid}/download", async (Guid invoiceId, InvoiceService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.DownloadAsync(invoiceId, cancellationToken);
                if (result is null)
                {
                    return Results.NotFound();
                }
                return Results.File(result.Value.File.Content, result.Value.Invoice.ContentType, result.Value.Invoice.OriginalFileName, enableRangeProcessing: true);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        }).WithName("DownloadInvoice");

        return endpoints;
    }
}
