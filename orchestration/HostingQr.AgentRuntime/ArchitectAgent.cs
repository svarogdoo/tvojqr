using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public sealed class ArchitectAgent : AgentBase
{
    public ArchitectAgent(IPromptProvider promptProvider) : base(promptProvider)
    {
    }

    public override AgentRole Role => AgentRole.Architect;

    protected override string PromptName => "ArchitectAgent";

    public override Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        List<PlanStep> steps =
        [
            new("arch-context", "Inspect existing boundaries", "Map the components, routes, and existing product constraints touched by the prompt before introducing new structure.", PlanStepType.Analysis),
            new("arch-scope", "Propose minimal architecture changes", "Prefer the smallest maintainable change that fits README direction and existing project rules.", PlanStepType.Design),
        ];

        List<string> assumptions =
        [
            "Repository guidance from README.md and AGENTS.md should be treated as hard constraints.",
            "New abstractions should only be introduced when they clearly improve maintainability.",
        ];

        List<string> risks = [];
        if (PromptClassifier.LooksLikeArchitectureWork(request.UserPrompt))
        {
            risks.Add("Architecture-facing prompts can expand quickly, so the orchestrator should enforce a narrow approved scope.");
        }

        AgentResponse response = new(
            Role,
            $"Architect agent recommends grounding the work in existing repo constraints and introducing only minimal structural changes. Instruction source: {GetInstructionSummary()}",
            assumptions,
            risks,
            steps,
            []);

        return Task.FromResult(response);
    }
}
