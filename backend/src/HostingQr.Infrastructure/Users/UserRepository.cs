using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Auth;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AuthUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select id, email, display_name as DisplayName
            from users
            where id = @Id;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AuthUserResponse>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<AuthUserResponse> UpsertAsync(Guid id, string email, string displayName, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into users (id, email, display_name)
            values (@Id, @Email, @DisplayName)
            on conflict (email) do update set
                display_name = excluded.display_name
            returning id, email, display_name as DisplayName;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<AuthUserResponse>(new CommandDefinition(sql, new
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
        }, cancellationToken: cancellationToken));
    }
}
