namespace HostingQr.AgentRuntime;

public static class AgentCatalog
{
    public static IReadOnlyList<IAgent> CreateDefaultAgents(IPromptProvider promptProvider)
    {
        return
        [
            new UiUxAgent(promptProvider),
            new ArchitectAgent(promptProvider),
            new EngineerAgent(promptProvider),
            new ReviewerAgent(promptProvider),
        ];
    }
}
