using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public sealed class EngineerAgent : AgentBase
{
    public EngineerAgent(IPromptProvider promptProvider) : base(promptProvider)
    {
    }

    public override AgentRole Role => AgentRole.Engineer;

    protected override string PromptName => "EngineerAgent";

    public override Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        List<PlanStep> steps =
        [
            new("eng-implement", "Prepare implementation slice", "Translate the approved scope into a small sequence of file changes, verification steps, and README updates.", PlanStepType.Implementation),
            new("eng-verify", "Verify with project checks", "Run the most relevant checks after edits and keep the result in the final report.", PlanStepType.Validation),
        ];

        List<string> assumptions =
        [
            request.Mode == WorkMode.DryRun
                ? "Phase 1 runs in dry-run mode, so no code should be edited automatically."
                : "Edit mode should only be entered after an explicit approval gate.",
            "Any implementation should preserve unrelated user changes already present in the worktree.",
        ];

        List<string> risks = [];
        if (!PromptClassifier.LooksLikeImplementationWork(request.UserPrompt))
        {
            risks.Add("The prompt is not clearly implementation-oriented, so engineering steps may need confirmation before execution.");
        }

        AgentResponse response = new(
            Role,
            $"Engineer agent converts the prompt into a concrete implementation and verification sequence, but stays read-only in Phase 1. Instruction source: {GetInstructionSummary()}",
            assumptions,
            risks,
            steps,
            []);

        return Task.FromResult(response);
    }
}
