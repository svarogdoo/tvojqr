using HostingQr.AgentContracts;
using HostingQr.AgentRuntime;
using HostingQr.Tools;

namespace HostingQr.Tests;

public sealed class OrchestrationCoordinatorTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"hostingqr-orchestrator-{Guid.NewGuid():N}");

    [Fact]
    public async Task RunAsync_ProducesPlanAndPersistsRunArtifacts()
    {
        Directory.CreateDirectory(_tempRoot);

        RepositoryContext repository = new(
            _tempRoot,
            "README",
            "AGENTS",
            "## main",
            "abc123 first commit",
            ["src/routes/+page.svelte", "src/routes/create-new/+page.svelte"]);

        IRepoInspector inspector = new FakeRepoInspector(repository);
        FileRunStore store = new(_tempRoot);
        FilePromptProvider promptProvider = new(Path.Combine(AppContext.BaseDirectory, "Prompts"));
        OrchestrationCoordinator coordinator = new(inspector, store, AgentCatalog.CreateDefaultAgents(promptProvider));

        OrchestrationResult result = await coordinator.RunAsync(_tempRoot, "Improve the upload page UX", WorkMode.DryRun);

        Assert.Equal("completed", result.Manifest.Status);
        Assert.Equal(4, result.AgentResponses.Count);
        Assert.NotEmpty(result.FinalPlan);
        Assert.True(Directory.Exists(result.Manifest.RunDirectory));
        Assert.Contains("Improve the upload page UX", result.FinalSummary);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }

    private sealed class FakeRepoInspector(RepositoryContext repository) : IRepoInspector
    {
        public Task<RepositoryContext> BuildContextAsync(string repositoryRoot, string prompt, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(repository with { RepositoryRoot = repositoryRoot });
        }
    }
}
