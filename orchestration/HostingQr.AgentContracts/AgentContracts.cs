namespace HostingQr.AgentContracts;

public enum AgentRole
{
    MainOrchestrator,
    UiUx,
    Architect,
    Engineer,
    Reviewer,
}

public enum WorkMode
{
    DryRun,
    Edit,
}

public enum PlanStepType
{
    Analysis,
    Design,
    Implementation,
    Validation,
    Review,
}

public enum ReviewFindingSeverity
{
    Info,
    Warning,
    Critical,
}

public sealed record PlanStep(string Id, string Title, string Description, PlanStepType Type);

public sealed record ReviewFinding(ReviewFindingSeverity Severity, string Title, string Detail);

public sealed record RepositoryContext(
    string RepositoryRoot,
    string ReadmeSummary,
    string AgentsSummary,
    string GitStatus,
    string RecentCommits,
    IReadOnlyList<string> RelevantFiles);

public sealed record AgentRequest(
    Guid RunId,
    AgentRole Role,
    string UserPrompt,
    WorkMode Mode,
    RepositoryContext Repository,
    IReadOnlyList<AgentResponse> PriorResponses);

public sealed record AgentResponse(
    AgentRole Role,
    string Summary,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> Risks,
    IReadOnlyList<PlanStep> ProposedSteps,
    IReadOnlyList<ReviewFinding> Findings);

public sealed record RunManifest(
    Guid RunId,
    DateTimeOffset StartedAt,
    string Prompt,
    WorkMode Mode,
    IReadOnlyList<AgentRole> InvokedAgents,
    string RunDirectory,
    string Status);

public sealed record OrchestrationResult(
    RunManifest Manifest,
    RepositoryContext Repository,
    IReadOnlyList<AgentResponse> AgentResponses,
    IReadOnlyList<PlanStep> FinalPlan,
    IReadOnlyList<ReviewFinding> Findings,
    string FinalSummary);
