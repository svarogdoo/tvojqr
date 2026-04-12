using HostingQr.Infrastructure.Data;
using HostingQr.Infrastructure.Configuration;

namespace HostingQr.Api.Tests;

public sealed class ConnectionStringResolverTests
{
    [Fact]
    public void Resolve_ConvertsPostgresUrlToNpgsqlConnectionString()
    {
        string result = ConnectionStringResolver.Resolve("postgresql://postgres:secret@db.example.com:5432/hostingqr?sslmode=Require");

        Assert.Contains("Host=db.example.com", result);
        Assert.Contains("Username=postgres", result);
        Assert.Contains("Password=secret", result);
        Assert.Contains("Database=hostingqr", result);
        Assert.Contains("SSL Mode=Require", result);
    }

    [Fact]
    public void Resolve_UsesPgEnvironmentVariablesWhenConnectionStringIsMissing()
    {
        string? previousHost = Environment.GetEnvironmentVariable("PGHOST");
        string? previousPort = Environment.GetEnvironmentVariable("PGPORT");
        string? previousDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        string? previousUser = Environment.GetEnvironmentVariable("PGUSER");
        string? previousPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

        try
        {
            Environment.SetEnvironmentVariable("PGHOST", "db.internal");
            Environment.SetEnvironmentVariable("PGPORT", "5432");
            Environment.SetEnvironmentVariable("PGDATABASE", "hostingqr");
            Environment.SetEnvironmentVariable("PGUSER", "postgres");
            Environment.SetEnvironmentVariable("PGPASSWORD", "secret");

            string result = ConnectionStringResolver.Resolve(new DatabaseOptions());

            Assert.Contains("Host=db.internal", result);
            Assert.Contains("Database=hostingqr", result);
            Assert.Contains("Username=postgres", result);
            Assert.Contains("Password=secret", result);
            Assert.Contains("SSL Mode=Require", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PGHOST", previousHost);
            Environment.SetEnvironmentVariable("PGPORT", previousPort);
            Environment.SetEnvironmentVariable("PGDATABASE", previousDatabase);
            Environment.SetEnvironmentVariable("PGUSER", previousUser);
            Environment.SetEnvironmentVariable("PGPASSWORD", previousPassword);
        }
    }
}
