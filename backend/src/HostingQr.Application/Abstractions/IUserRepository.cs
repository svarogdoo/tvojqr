using HostingQr.Application.Auth;

namespace HostingQr.Application.Abstractions;

public interface IUserRepository
{
    Task<AuthUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AuthUserResponse> UpsertAsync(Guid id, string email, string displayName, CancellationToken cancellationToken = default);
}
