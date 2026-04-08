using HostingQr.AgentContracts;
using HostingQr.Tools;

namespace HostingQr.AgentRuntime;

public sealed class OrchestrationCoordinator
{
    private readonly IRepoInspector _repoInspector;
    private readonly IRunStore _runStore;
    private readonly IReadOnlyList<IAgent> _agents;

    public OrchestrationCoordinator(IRepoInspector repoInspector, IRunStore runStore, IEnumerable<IAgent> agents)
    {
        _repoInspector = repoInspector;
        _runStore = runStore;
        _agents = agents.ToArray();
    }

    public async Task<OrchestrationResult> RunAsync(string repositoryRoot, string prompt, WorkMode mode = WorkMode.DryRun, CancellationToken cancellationToken = default)
    {
        Guid runId = Guid.NewGuid();
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        string runDirectory = await _runStore.CreateRunDirectoryAsync(runId, startedAt, cancellationToken);
        RepositoryContext repository = await _repoInspector.BuildContextAsync(repositoryRoot, prompt, cancellationToken);

        RunManifest manifest = new(
            runId,
            startedAt,
            prompt,
            mode,
            _agents.Select(agent => agent.Role).ToArray(),
            runDirectory,
            "running");

        await _runStore.SaveManifestAsync(manifest, cancellationToken);

        List<AgentResponse> responses = [];
        foreach (IAgent agent in _agents)
        {
            AgentRequest request = new(runId, agent.Role, prompt, mode, repository, responses.ToArray());
            AgentResponse response = await agent.ExecuteAsync(request, cancellationToken);
            responses.Add(response);
        }

        IReadOnlyList<PlanStep> finalPlan = responses
            .SelectMany(response => response.ProposedSteps)
            .GroupBy(step => step.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        IReadOnlyList<ReviewFinding> findings = responses
            .SelectMany(response => response.Findings)
            .ToArray();

        string summary = BuildSummary(prompt, mode, repository, responses, finalPlan, findings);

        RunManifest completedManifest = manifest with { Status = "completed" };
        OrchestrationResult result = new(completedManifest, repository, responses, finalPlan, findings, summary);

        await _runStore.SaveManifestAsync(completedManifest, cancellationToken);
        await _runStore.SaveResultAsync(result, cancellationToken);

        return result;
    }

    private static string BuildSummary(
        string prompt,
        WorkMode mode,
        RepositoryContext repository,
        IReadOnlyList<AgentResponse> responses,
        IReadOnlyList<PlanStep> plan,
        IReadOnlyList<ReviewFinding> findings)
    {
        return $"Prompt: {prompt}{Environment.NewLine}" +
               $"Mode: {mode}{Environment.NewLine}" +
               $"Relevant files: {string.Join(", ", repository.RelevantFiles)}{Environment.NewLine}" +
               $"Agents consulted: {string.Join(", ", responses.Select(response => response.Role))}{Environment.NewLine}" +
               $"Plan steps: {plan.Count}{Environment.NewLine}" +
               $"Review findings: {findings.Count}";
    }
}
