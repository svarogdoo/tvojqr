using HostingQr.Application.Invitations;
using Microsoft.AspNetCore.Authorization;

namespace HostingQr.Api.Endpoints;

public static class InvitationEndpoints
{
    public static IEndpointRouteBuilder MapInvitationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/invitations").WithTags("Invitations");

        group.MapGet("/{token}", [AllowAnonymous] async (string token, ProjectInvitationService service, CancellationToken cancellationToken) =>
        {
            InvitationDetailsResponse? invitation = await service.GetDetailsAsync(token, cancellationToken);
            return invitation is null ? Results.NotFound() : Results.Ok(invitation);
        }).WithName("GetInvitationDetails");

        group.MapPost("/{token}/accept", [Authorize] async (string token, ProjectInvitationService service, CancellationToken cancellationToken) =>
        {
            InvitationAcceptanceResult result = await service.AcceptAsync(token, cancellationToken);
            return result switch
            {
                InvitationAcceptanceResult.Accepted => Results.NoContent(),
                InvitationAcceptanceResult.EmailMismatch => Results.Json(new { message = "Sign in with the Google account that was invited." }, statusCode: StatusCodes.Status403Forbidden),
                InvitationAcceptanceResult.ExpiredOrUsed => Results.Conflict(new { message = "This invitation has expired or has already been used." }),
                _ => Results.NotFound(),
            };
        }).WithName("AcceptProjectInvitation");

        return endpoints;
    }
}
