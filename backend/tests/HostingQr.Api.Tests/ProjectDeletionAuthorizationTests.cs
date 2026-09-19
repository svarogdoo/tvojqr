using HostingQr.Application.Abstractions;
using HostingQr.Application.Projects;
using HostingQr.Domain.Projects;

namespace HostingQr.Api.Tests;

public sealed class ProjectDeletionAuthorizationTests
{
    private static readonly Guid OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Delete_DoesNotTouchRepository_WhenProjectIsNotAccessible()
    {
        TrackingProjectRepository repository = new();
        ProjectService service = CreateService(repository, new AccessService(null));

        bool deleted = await service.DeleteProjectAsync(ProjectId);

        Assert.False(deleted);
        Assert.False(repository.DeleteCalled);
    }

    [Fact]
    public async Task Delete_UsesActualOwnerScope_ForAdminAccessibleProject()
    {
        TrackingProjectRepository repository = new();
        ProjectWithSlug project = new() { Id = ProjectId, OwnerUserId = OwnerId, Name = "Menu", Slug = "menu" };
        ProjectService service = CreateService(repository, new AccessService(project));

        bool deleted = await service.DeleteProjectAsync(ProjectId);

        Assert.True(deleted);
        Assert.Equal(OwnerId, repository.DeleteOwnerId);
    }

    private static ProjectService CreateService(IProjectRepository repository, IProjectAccessService accessService) =>
        new(null!, null!, repository, null!, null!, null!, null!, null!, null!, accessService);

    private sealed class AccessService(ProjectWithSlug? project) : IProjectAccessService
    {
        public Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(project is not null);
        public Task<ProjectWithSlug?> GetAccessibleProjectAsync(Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult(project?.Id == projectId ? project : null);
    }

    private sealed class TrackingProjectRepository : IProjectRepository
    {
        public bool DeleteCalled { get; private set; }
        public Guid? DeleteOwnerId { get; private set; }
        public Task<bool> DeleteAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default)
        {
            DeleteCalled = true;
            DeleteOwnerId = ownerUserId;
            return Task.FromResult(ownerUserId == OwnerId && projectId == ProjectId);
        }
        public Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, string backgroundColor, string menuType, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, string backgroundColor, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ProjectWithSlug?> UpdateStatusAsync(Guid ownerUserId, Guid projectId, string status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
