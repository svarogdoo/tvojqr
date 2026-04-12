using HostingQr.Infrastructure.Configuration;
using Npgsql;

namespace HostingQr.Infrastructure.Data;

public static class ConnectionStringResolver
{
    public static string Resolve(DatabaseOptions options)
    {
        return Resolve(options.ConnectionString ?? throw new InvalidOperationException("Database connection string is not configured."));
    }

    public static string Resolve(string rawConnectionString)
    {
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        string trimmed = rawConnectionString.Trim();
        trimmed = trimmed.Trim('"', '\'', '“', '”');

        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return ConvertUrlToNpgsqlConnectionString(trimmed);
        }

        return trimmed;
    }

    private static string ConvertUrlToNpgsqlConnectionString(string connectionUrl)
    {
        Uri uri = new(connectionUrl);
        string[] userInfoParts = uri.UserInfo.Split(':', 2);

        string username = Uri.UnescapeDataString(userInfoParts[0]);
        string password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
        string database = uri.AbsolutePath.Trim('/');

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = username,
            Password = password,
            Database = database,
            SslMode = GetDefaultSslMode(uri.Host),
        };

        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            string query = uri.Query.TrimStart('?');
            foreach (string pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] kvp = pair.Split('=', 2);
                string key = Uri.UnescapeDataString(kvp[0]);
                string value = kvp.Length > 1 ? Uri.UnescapeDataString(kvp[1]) : string.Empty;

                switch (key.ToLowerInvariant())
                {
                    case "sslmode":
                        if (Enum.TryParse<SslMode>(value, true, out var sslMode))
                        {
                            builder.SslMode = sslMode;
                        }
                        break;
                    default:
                        builder[key] = value;
                        break;
                }
            }
        }

        return builder.ConnectionString;
    }

    private static SslMode GetDefaultSslMode(string host)
    {
        return host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase)
            ? SslMode.Disable
            : SslMode.Require;
    }
}
