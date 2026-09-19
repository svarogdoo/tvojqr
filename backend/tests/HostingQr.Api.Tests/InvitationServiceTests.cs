using HostingQr.Application.Abstractions;
using HostingQr.Application.Invitations;
using HostingQr.Domain.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace HostingQr.Api.Tests;

public sealed class InvitationServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ProjectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Create_StoresOnlyHash_AndNormalizesEmail()
    {
        InMemoryInvitationRepository repository = new();
        CapturingEmailSender sender = new();
        ProjectInvitationService service = CreateService(new CurrentUserContext(AdminId, "admin@example.com"), repository, sender);

        ProjectInvitationResponse response = await service.CreateAsync(ProjectId, new CreateProjectInvitationRequest(" Client@Example.COM "));

        Assert.Equal("client@example.com", repository.Invitation!.InvitedEmailNormalized);
        string token = sender.InviteUrl!.Segments[^1];
        Assert.NotEqual(token, repository.Invitation.TokenHash);
        Assert.Equal(ProjectInvitationService.HashToken(token), repository.Invitation.TokenHash);
        Assert.Equal(ProjectId, response.ProjectId);
    }

    [Fact]
    public async Task Accept_RequiresMatchingNormalizedEmail_AndIsOneUse()
    {
        InMemoryInvitationRepository repository = new();
        CapturingEmailSender sender = new();
        ProjectInvitationService adminService = CreateService(new CurrentUserContext(AdminId, "admin@example.com"), repository, sender);
        await adminService.CreateAsync(ProjectId, new CreateProjectInvitationRequest("client@example.com"));
        string token = sender.InviteUrl!.Segments[^1];

        ProjectInvitationService wrongUserService = CreateService(new CurrentUserContext(ClientId, "other@example.com"), repository, sender);
        Assert.Equal(InvitationAcceptanceResult.EmailMismatch, await wrongUserService.AcceptAsync(token));

        ProjectInvitationService clientService = CreateService(new CurrentUserContext(ClientId, "CLIENT@example.com"), repository, sender);
        Assert.Equal(InvitationAcceptanceResult.Accepted, await clientService.AcceptAsync(token));
        Assert.Equal(InvitationAcceptanceResult.ExpiredOrUsed, await clientService.AcceptAsync(token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("@")]
    [InlineData("client@")]
    [InlineData("Client <client@example.com>")]
    public async Task Create_RejectsMalformedEmail(string email)
    {
        InMemoryInvitationRepository repository = new();
        ProjectInvitationService service = CreateService(
            new CurrentUserContext(AdminId, "admin@example.com"), repository, new CapturingEmailSender());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(ProjectId, new CreateProjectInvitationRequest(email)));
        Assert.Null(repository.Invitation);
    }

    [Fact]
    public async Task Create_RevokesInvitation_WhenDeliveryFails()
    {
        InMemoryInvitationRepository repository = new();
        ProjectInvitationService service = CreateService(
            new CurrentUserContext(AdminId, "admin@example.com"), repository, new FailingEmailSender());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(ProjectId, new CreateProjectInvitationRequest("client@example.com")));
        Assert.NotNull(repository.Invitation?.RevokedAt);
    }

    private static ProjectInvitationService CreateService(ICurrentUserContext user, InMemoryInvitationRepository repository, IInvitationEmailSender sender)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auth:FrontendBaseUrl"] = "https://hostingqr.test",
        }).Build();
        return new ProjectInvitationService(user, new AccessService(user), repository, sender, TimeProvider.System, NullLogger<ProjectInvitationService>.Instance, configuration);
    }

    private sealed class CurrentUserContext(Guid id, string email) : ICurrentUserContext
    {
        public Guid GetCurrentUserId() => id;
        public CurrentUser GetCurrentUser() => new(id, email, "User");
    }

    private sealed class AccessService(ICurrentUserContext user) : IProjectAccessService
    {
        public Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(user.GetCurrentUserId() == AdminId);
        public Task<ProjectWithSlug?> GetAccessibleProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectWithSlug?>(projectId == ProjectId ? new ProjectWithSlug { Id = ProjectId, OwnerUserId = AdminId, Name = "Cafe Menu", Slug = "cafe" } : null);
    }

    private sealed class CapturingEmailSender : IInvitationEmailSender
    {
        public bool IsConfigured => true;
        public Uri? InviteUrl { get; private set; }
        public Task SendProjectInvitationAsync(string recipientEmail, string projectName, Uri inviteUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            InviteUrl = inviteUrl;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingEmailSender : IInvitationEmailSender
    {
        public bool IsConfigured => true;
        public Task SendProjectInvitationAsync(string recipientEmail, string projectName, Uri inviteUrl, DateTimeOffset expiresAt, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("SMTP failed.");
    }

    private sealed class InMemoryInvitationRepository : IProjectInvitationRepository
    {
        public ProjectInvitationRecord? Invitation { get; private set; }

        public Task<ProjectInvitationRecord> CreateAsync(Guid projectId, Guid inviterUserId, string normalizedEmail, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            Invitation = new ProjectInvitationRecord(Guid.NewGuid(), projectId, inviterUserId, normalizedEmail, tokenHash, expiresAt, null, null, DateTimeOffset.UtcNow, "Cafe Menu");
            return Task.FromResult(Invitation);
        }

        public Task<ProjectInvitationRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
            Task.FromResult(Invitation?.TokenHash == tokenHash ? Invitation : null);

        public Task<bool> RevokeAsync(Guid invitationId, Guid inviterUserId, CancellationToken cancellationToken = default)
        {
            if (Invitation?.Id != invitationId || Invitation.AcceptedAt is not null) return Task.FromResult(false);
            Invitation = Invitation with { RevokedAt = DateTimeOffset.UtcNow };
            return Task.FromResult(true);
        }

        public Task<InvitationAcceptanceResult> AcceptAsync(string tokenHash, Guid acceptingUserId, string normalizedEmail, string entitlementTier, CancellationToken cancellationToken = default)
        {
            if (Invitation is null || Invitation.TokenHash != tokenHash) return Task.FromResult(InvitationAcceptanceResult.NotFound);
            if (Invitation.AcceptedAt is not null || Invitation.RevokedAt is not null || Invitation.ExpiresAt <= DateTimeOffset.UtcNow) return Task.FromResult(InvitationAcceptanceResult.ExpiredOrUsed);
            if (Invitation.InvitedEmailNormalized != normalizedEmail) return Task.FromResult(InvitationAcceptanceResult.EmailMismatch);
            Invitation = Invitation with { AcceptedAt = DateTimeOffset.UtcNow };
            return Task.FromResult(InvitationAcceptanceResult.Accepted);
        }
    }
}
