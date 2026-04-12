using System.Data;
using HostingQr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HostingQr.Infrastructure.Data;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public NpgsqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public IDbConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return new NpgsqlConnection(ConnectionStringResolver.Resolve(_options));
        }

        return new NpgsqlConnection(ConnectionStringResolver.Resolve(_options));
    }
}
