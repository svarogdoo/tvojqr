namespace HostingQr.Application.Abstractions;

public sealed record CurrentUser(Guid Id, string Email, string DisplayName);

public interface ICurrentUserContext
{
    Guid GetCurrentUserId();

    CurrentUser GetCurrentUser();
}
