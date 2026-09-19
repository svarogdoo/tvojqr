using System.Net;
using System.Net.Mail;
using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HostingQr.Infrastructure.Invitations;

public sealed class SmtpInvitationEmailSender : IInvitationEmailSender
{
    private readonly SmtpOptions _options;
    public SmtpInvitationEmailSender(IOptions<SmtpOptions> options) => _options = options.Value;
    public bool IsConfigured => _options.IsConfigured();

    public async Task SendProjectInvitationAsync(string recipientEmail, string projectName, Uri inviteUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("SMTP is not configured.");
        }

        using MailMessage message = new()
        {
            From = new MailAddress(_options.FromAddress!, _options.FromName),
            Subject = $"Your HostingQr menu: {projectName}",
            Body = $"You have been invited to manage '{projectName}'.\n\nAccept the invitation: {inviteUrl}\n\nThis invitation expires {expiresAt:O}.",
            IsBodyHtml = false,
        };
        message.To.Add(recipientEmail);
        using SmtpClient client = new(_options.Host!, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_options.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.Username, _options.Password),
        };
        await client.SendMailAsync(message, cancellationToken);
    }
}
