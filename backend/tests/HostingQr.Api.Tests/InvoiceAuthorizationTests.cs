using HostingQr.Application.Abstractions;
using HostingQr.Application.Invoices;

namespace HostingQr.Api.Tests;

public sealed class InvoiceAuthorizationTests
{
    private static readonly Guid ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherClientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid InvoiceId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Download_AllowsInvoiceOwner()
    {
        InvoiceService service = CreateService(ClientId, isAdmin: false);

        var result = await service.DownloadAsync(InvoiceId);

        Assert.NotNull(result);
        await result.Value.File.DisposeAsync();
    }

    [Fact]
    public async Task Download_ForbidsAnotherClient()
    {
        InvoiceService service = CreateService(OtherClientId, isAdmin: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DownloadAsync(InvoiceId));
    }

    [Fact]
    public async Task Download_AllowsAdmin()
    {
        InvoiceService service = CreateService(OtherClientId, isAdmin: true);

        var result = await service.DownloadAsync(InvoiceId);

        Assert.NotNull(result);
        await result.Value.File.DisposeAsync();
    }

    private static InvoiceService CreateService(Guid currentUserId, bool isAdmin) => new(
        new CurrentUserContext(currentUserId),
        new AccessService(isAdmin),
        new InvoiceRepository(),
        new Storage());

    private sealed class CurrentUserContext(Guid userId) : ICurrentUserContext
    {
        public Guid GetCurrentUserId() => userId;
        public CurrentUser GetCurrentUser() => new(userId, "user@example.com", "User");
    }

    private sealed class AccessService(bool isAdmin) : IProjectAccessService
    {
        public Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(isAdmin);
        public Task<HostingQr.Domain.Projects.ProjectWithSlug?> GetAccessibleProjectAsync(Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult<HostingQr.Domain.Projects.ProjectWithSlug?>(null);
    }

    private sealed class InvoiceRepository : IInvoiceRepository
    {
        private static readonly InvoiceRecord Invoice = new(InvoiceId, ClientId, Guid.Empty, new DateOnly(2026, 9, 1), "invoice.pdf", "application/pdf", "invoice.pdf", 8, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        public Task<bool> ClientExistsAsync(Guid clientUserId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<InvoiceRecord> CreateAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default) => Task.FromResult(invoice);
        public Task<IReadOnlyList<InvoiceRecord>> ListByClientAsync(Guid clientUserId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<InvoiceRecord>>([Invoice]);
        public Task<InvoiceRecord?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default) => Task.FromResult<InvoiceRecord?>(invoiceId == InvoiceId ? Invoice : null);
        public Task<bool> DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class Storage : IPrivateInvoiceStorage
    {
        public Task<StoredPrivateFile> SaveAsync(Guid invoiceId, Stream content, CancellationToken cancellationToken = default) => Task.FromResult(new StoredPrivateFile("invoice.pdf", content.Length));
        public Task<PrivateFile?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) => Task.FromResult<PrivateFile?>(new PrivateFile(new MemoryStream("%PDF-1.7"u8.ToArray()), "application/pdf", 8));
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
