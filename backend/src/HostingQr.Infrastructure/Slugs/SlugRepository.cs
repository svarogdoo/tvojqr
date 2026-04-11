using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Slugs;

public sealed class SlugRepository : ISlugRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SlugRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = "select exists(select 1 from slugs where slug = @Slug);";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Slug = slug }, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(command);
    }
}
