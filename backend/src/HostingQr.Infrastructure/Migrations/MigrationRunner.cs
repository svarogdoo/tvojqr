using HostingQr.Infrastructure.Configuration;
using HostingQr.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Reflection;

namespace HostingQr.Infrastructure.Migrations;

public sealed class MigrationRunner
{
    private readonly DatabaseOptions _databaseOptions;
    private readonly DevelopmentUserOptions _developmentUserOptions;
    private readonly MigrationOptions _migrationOptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(
        IOptions<DatabaseOptions> databaseOptions,
        IOptions<DevelopmentUserOptions> developmentUserOptions,
        IOptions<MigrationOptions> migrationOptions,
        IHostEnvironment environment,
        ILogger<MigrationRunner> logger)
    {
        _databaseOptions = databaseOptions.Value;
        _developmentUserOptions = developmentUserOptions.Value;
        _migrationOptions = migrationOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_migrationOptions.RunOnStartup)
        {
            _logger.LogInformation("Database migrations are disabled for this environment.");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_databaseOptions.ConnectionString))
        {
            _logger.LogWarning("Database migrations were requested, but no connection string is configured.");
            return Task.CompletedTask;
        }

        return RunMigrationsInternalAsync(cancellationToken);
    }

    private async Task RunMigrationsInternalAsync(CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = new(ConnectionStringResolver.Resolve(_databaseOptions));
        await connection.OpenAsync(cancellationToken);

        const string historySql = """
            create table if not exists schema_migrations (
                script_name text primary key,
                applied_at timestamptz not null default now()
            );
            """;

        await using (NpgsqlCommand historyCommand = new(historySql, connection))
        {
            await historyCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        string[] appliedScripts = await GetAppliedScriptsAsync(connection, cancellationToken);
        HashSet<string> applied = appliedScripts.ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assembly assembly = typeof(MigrationRunner).Assembly;
        string[] scriptNames = assembly
            .GetManifestResourceNames()
            .Where(name => name.Contains("Migrations.Scripts.", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (string scriptName in scriptNames)
        {
            if (applied.Contains(scriptName))
            {
                continue;
            }

            await using Stream stream = assembly.GetManifestResourceStream(scriptName)
                ?? throw new InvalidOperationException($"Unable to load migration script '{scriptName}'.");
            using StreamReader reader = new(stream);
            string sql = await reader.ReadToEndAsync(cancellationToken);
            sql = ReplaceVariables(sql);

            await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using (NpgsqlCommand migrationCommand = new(sql, connection, transaction))
            {
                await migrationCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (NpgsqlCommand insertHistoryCommand = new("insert into schema_migrations (script_name) values (@scriptName);", connection, transaction))
            {
                insertHistoryCommand.Parameters.AddWithValue("scriptName", scriptName);
                await insertHistoryCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        _logger.LogInformation("Database migrations completed for {EnvironmentName}.", _environment.EnvironmentName);
    }

    private async Task<string[]> GetAppliedScriptsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = "select script_name from schema_migrations;";
        List<string> scripts = [];

        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            scripts.Add(reader.GetString(0));
        }

        return scripts.ToArray();
    }

    private string ReplaceVariables(string sql)
    {
        return sql
            .Replace("${DevelopmentUserId}", _developmentUserOptions.Id.ToString(), StringComparison.Ordinal)
            .Replace("${DevelopmentUserEmail}", _developmentUserOptions.Email.Replace("'", "''", StringComparison.Ordinal), StringComparison.Ordinal)
            .Replace("${DevelopmentUserDisplayName}", _developmentUserOptions.DisplayName.Replace("'", "''", StringComparison.Ordinal), StringComparison.Ordinal);
    }
}
