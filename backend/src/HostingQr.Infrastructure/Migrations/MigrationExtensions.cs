using Microsoft.Extensions.DependencyInjection;

namespace HostingQr.Infrastructure.Migrations;

public static class MigrationExtensions
{
    public static async Task RunDatabaseMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        MigrationRunner runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
        await runner.RunAsync(cancellationToken);
    }
}
