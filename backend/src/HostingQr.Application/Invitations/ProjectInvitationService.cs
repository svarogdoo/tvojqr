using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Billing;
using Microsoft.Extensions.Logging;

namespace HostingQr.Application.Invitations;

public sealed class ProjectInvitationService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IProjectAccessService _projectAccessService;
    private readonly IProjectInvitationRepository _repository;
    private readonly IInvitationEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProjectInvitationService> _logger;
    private readonly string _frontendBaseUrl;

    public ProjectInvitationService(
        ICurrentUserContext currentUserContext,
        IProjectAccessService projectAccessService,
        IProjectInvitationRepository repository,
        IInvitationEmailSender emailSender,
        TimeProvider timeProvider,
        ILogger<ProjectInvitationService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _currentUserContext = currentUserContext;
        _projectAccessService = projectAccessService;
        _repository = repository;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
        _logger = logger;
        _frontendBaseUrl = configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:5173";
    }

    public async Task<ProjectInvitationResponse> CreateAsync(Guid projectId, CreateProjectInvitationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        Domain.Projects.ProjectWithSlug? project = await _projectAccessService.GetAccessibleProjectAsync(projectId, cancellationToken);
        Guid adminId = _currentUserContext.GetCurrentUserId();
        if (project is null || project.OwnerUserId != adminId)
        {
            throw new KeyNotFoundException("Project was not found or is no longer owned by the admin.");
        }

        if (!_emailSender.IsConfigured)
        {
            throw new InvalidOperationException("Invitation email is unavailable because SMTP is not configured.");
        }

        string normalizedEmail = NormalizeEmail(request.Email);
        int expiryDays = request.ExpiresInDays is >= 1 and <= 30 ? request.ExpiresInDays : throw new ArgumentException("Invitation expiry must be between 1 and 30 days.");
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        string tokenHash = HashToken(token);
        DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddDays(expiryDays);
        ProjectInvitationRecord invitation = await _repository.CreateAsync(projectId, adminId, normalizedEmail, tokenHash, expiresAt, cancellationToken);

        try
        {
            Uri inviteUrl = new($"{_frontendBaseUrl.TrimEnd('/')}/invitations/{Uri.EscapeDataString(token)}");
            await _emailSender.SendProjectInvitationAsync(normalizedEmail, project.Name, inviteUrl, expiresAt, cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                using CancellationTokenSource cleanupTimeout = new(TimeSpan.FromSeconds(5));
                await _repository.RevokeAsync(invitation.Id, adminId, cleanupTimeout.Token);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(cleanupException, "Could not revoke undelivered project invitation {InvitationId}.", invitation.Id);
            }
            throw new InvalidOperationException("Invitation email could not be sent.", exception);
        }

        return Map(invitation);
    }

    public async Task<InvitationDetailsResponse?> GetDetailsAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        ProjectInvitationRecord? invitation = await _repository.GetByTokenHashAsync(HashToken(token), cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        bool available = invitation.AcceptedAt is null && invitation.RevokedAt is null && invitation.ExpiresAt > _timeProvider.GetUtcNow();
        return new InvitationDetailsResponse(invitation.ProjectName, invitation.ExpiresAt, available);
    }

    public Task<bool> RevokeAsync(Guid invitationId, CancellationToken cancellationToken = default) =>
        RevokeInternalAsync(invitationId, cancellationToken);

    public async Task<InvitationAcceptanceResult> AcceptAsync(string token, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserContext.GetCurrentUser();
        return await _repository.AcceptAsync(HashToken(token), currentUser.Id, NormalizeEmail(currentUser.Email), BillingTier.Plus, cancellationToken);
    }

    private async Task<bool> RevokeInternalAsync(Guid invitationId, CancellationToken cancellationToken)
    {
        if (!await _projectAccessService.IsCurrentUserAdminAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        return await _repository.RevokeAsync(invitationId, _currentUserContext.GetCurrentUserId(), cancellationToken);
    }

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email.Trim(), out MailAddress? address))
        {
            throw new ArgumentException("A valid email address is required.");
        }

        string normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 320 || !string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A valid email address is required.");
        }

        return normalized;
    }

    public static string HashToken(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static ProjectInvitationResponse Map(ProjectInvitationRecord invitation) => new(
        invitation.Id,
        invitation.ProjectId,
        invitation.ProjectName,
        invitation.ExpiresAt,
        invitation.AcceptedAt,
        invitation.RevokedAt,
        invitation.CreatedAt);
}
