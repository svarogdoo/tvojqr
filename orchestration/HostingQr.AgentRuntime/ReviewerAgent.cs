using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public sealed class ReviewerAgent : AgentBase
{
    public ReviewerAgent(IPromptProvider promptProvider) : base(promptProvider)
    {
    }

    public override AgentRole Role => AgentRole.Reviewer;

    protected override string PromptName => "ReviewerAgent";

    public override Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        List<PlanStep> steps =
        [
            new("review-risks", "Review risks before execution", "Identify likely regressions, testing gaps, and places where scope may drift.", PlanStepType.Review),
            new("review-tests", "Define validation expectations", "State the minimum tests or checks that must run for the task to be considered complete.", PlanStepType.Validation),
        ];

        List<ReviewFinding> findings =
        [
            new(ReviewFindingSeverity.Info, "Testing expectation", "Every approved implementation should include at least one verification path, even if it is only typechecking or a targeted build/test command."),
        ];

        List<string> risks =
        [
            "Without an approval gate, a future edit-capable engineer agent could overreach beyond the requested scope.",
        ];

        AgentResponse response = new(
            Role,
            $"Reviewer agent emphasizes explicit validation and approval gates before any future edit execution is enabled. Instruction source: {GetInstructionSummary()}",
            ["Review output should remain read-only and independent from implementation."],
            risks,
            steps,
            findings);

        return Task.FromResult(response);
    }
}
