using System.Text.Json;
using HostingQr.AgentContracts;
using HostingQr.AgentRuntime;

namespace HostingQr.Tests;

public sealed class FileRunStoreTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"hostingqr-runs-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveResultAsync_WritesManifestAndResultFiles()
    {
        Directory.CreateDirectory(_tempRoot);
        FileRunStore store = new(_tempRoot);
        Guid runId = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string runDirectory = await store.CreateRunDirectoryAsync(runId, now);

        RunManifest manifest = new(runId, now, "test prompt", WorkMode.DryRun, [AgentRole.UiUx], runDirectory, "completed");
        RepositoryContext repository = new(_tempRoot, "readme", "agents", "status", "commits", ["src/routes/+page.svelte"]);
        OrchestrationResult result = new(manifest, repository, [], [], [], "summary");

        await store.SaveManifestAsync(manifest);
        await store.SaveResultAsync(result);

        Assert.True(File.Exists(Path.Combine(runDirectory, "manifest.json")));
        Assert.True(File.Exists(Path.Combine(runDirectory, "result.json")));

        string raw = await File.ReadAllTextAsync(Path.Combine(runDirectory, "result.json"));
        OrchestrationResult? persisted = JsonSerializer.Deserialize<OrchestrationResult>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(persisted);
        Assert.Equal("summary", persisted.FinalSummary);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }
}
