namespace HostingQr.AgentRuntime;

public interface IPromptProvider
{
    string GetPrompt(string promptName);
}
