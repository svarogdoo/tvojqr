using HostingQr.AgentContracts;
using HostingQr.AgentRuntime;
using HostingQr.Tools;

string[] arguments = args.Length > 0 && string.Equals(args[0], "ask", StringComparison.OrdinalIgnoreCase)
    ? args[1..]
    : args;

if (arguments.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run --project orchestration/HostingQr.Orchestrator -- ask \"your prompt\"");
    return 1;
}

string prompt = string.Join(' ', arguments).Trim();
string repositoryRoot = Directory.GetCurrentDirectory();
string promptRoot = Path.Combine(repositoryRoot, "orchestration", "HostingQr.AgentRuntime", "Prompts");

FileRunStore runStore = new(repositoryRoot);
FileSystemRepoInspector repoInspector = new();
FilePromptProvider promptProvider = new(promptRoot);
OrchestrationCoordinator coordinator = new(repoInspector, runStore, AgentCatalog.CreateDefaultAgents(promptProvider));

OrchestrationResult result = await coordinator.RunAsync(repositoryRoot, prompt, WorkMode.DryRun);

Console.WriteLine("HostingQr local orchestrator");
Console.WriteLine();
Console.WriteLine(result.FinalSummary);
Console.WriteLine();
Console.WriteLine($"Run directory: {result.Manifest.RunDirectory}");
Console.WriteLine();

foreach (AgentResponse response in result.AgentResponses)
{
    Console.WriteLine($"[{response.Role}] {response.Summary}");
    foreach (PlanStep step in response.ProposedSteps)
    {
        Console.WriteLine($"  - {step.Title}: {step.Description}");
    }

    if (response.Risks.Count > 0)
    {
        Console.WriteLine("  Risks:");
        foreach (string risk in response.Risks)
        {
            Console.WriteLine($"  - {risk}");
        }
    }

    Console.WriteLine();
}

return 0;
