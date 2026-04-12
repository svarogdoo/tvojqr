using HostingQr.Infrastructure.Data;

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
    public void Resolve_DisablesSslForRailwayInternalHosts()
    {
        string result = ConnectionStringResolver.Resolve("postgresql://postgres:secret@postgres.railway.internal:5432/railway");

        Assert.Contains("Host=postgres.railway.internal", result);
        Assert.Contains("SSL Mode=Disable", result);
    }
}
