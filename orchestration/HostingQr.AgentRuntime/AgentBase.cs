using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public abstract class AgentBase : IAgent
{
    private readonly IPromptProvider _promptProvider;

    protected AgentBase(IPromptProvider promptProvider)
    {
        _promptProvider = promptProvider;
    }

    public abstract AgentRole Role { get; }

    protected abstract string PromptName { get; }

    protected string GetInstructionSummary()
    {
        string prompt = _promptProvider.GetPrompt(PromptName);
        return string.Join(' ', prompt.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Take(4));
    }

    public abstract Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default);
}
