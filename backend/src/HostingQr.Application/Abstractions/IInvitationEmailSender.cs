namespace HostingQr.Application.Abstractions;

public interface IInvitationEmailSender
{
    bool IsConfigured { get; }
    Task SendProjectInvitationAsync(string recipientEmail, string projectName, Uri inviteUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
}
