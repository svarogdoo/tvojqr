using System.Text.Json;
using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public sealed class FileRunStore : IRunStore
{
    private readonly string _runsRoot;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public FileRunStore(string repositoryRoot)
    {
        _runsRoot = Path.Combine(repositoryRoot, ".orchestration", "runs");
    }

    public Task<string> CreateRunDirectoryAsync(Guid runId, DateTimeOffset startedAt, CancellationToken cancellationToken = default)
    {
        string directoryName = $"{startedAt:yyyyMMdd-HHmmss}-{runId.ToString()[..8]}";
        string directoryPath = Path.Combine(_runsRoot, directoryName);
        Directory.CreateDirectory(directoryPath);
        return Task.FromResult(directoryPath);
    }

    public Task SaveManifestAsync(RunManifest manifest, CancellationToken cancellationToken = default)
    {
        return SaveAsync(Path.Combine(manifest.RunDirectory, "manifest.json"), manifest, cancellationToken);
    }

    public Task SaveResultAsync(OrchestrationResult result, CancellationToken cancellationToken = default)
    {
        return SaveAsync(Path.Combine(result.Manifest.RunDirectory, "result.json"), result, cancellationToken);
    }

    private async Task SaveAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, _jsonOptions, cancellationToken);
    }
}
