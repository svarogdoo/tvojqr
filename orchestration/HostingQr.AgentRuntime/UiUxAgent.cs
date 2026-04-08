using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public sealed class UiUxAgent : AgentBase
{
    public UiUxAgent(IPromptProvider promptProvider) : base(promptProvider)
    {
    }

    public override AgentRole Role => AgentRole.UiUx;

    protected override string PromptName => "UiUxAgent";

    public override Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        List<PlanStep> steps =
        [
            new("ui-review", "Review primary user flow", "Check the main screens involved in the prompt and note UX friction, hierarchy problems, and missing affordances.", PlanStepType.Analysis),
            new("ui-criteria", "Define UI acceptance criteria", "Write explicit acceptance criteria for layout clarity, responsiveness, and user feedback before implementation starts.", PlanStepType.Design),
        ];

        List<string> assumptions =
        [
            "The app should keep a clean, minimal visual language consistent with recent homepage changes.",
            "Mobile readability is a required quality bar for any UI work.",
        ];

        List<string> risks = [];
        if (!PromptClassifier.LooksLikeUiWork(request.UserPrompt))
        {
            risks.Add("The prompt is not explicitly UI-focused, so UX recommendations may need scope trimming before implementation.");
        }

        AgentResponse response = new(
            Role,
            $"UI/UX agent recommends clarifying the primary user flow and defining acceptance criteria before changes are applied. Instruction source: {GetInstructionSummary()}",
            assumptions,
            risks,
            steps,
            []);

        return Task.FromResult(response);
    }
}
