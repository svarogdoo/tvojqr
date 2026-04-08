using HostingQr.AgentRuntime;

namespace HostingQr.Tests;

public sealed class PromptProviderTests
{
    [Fact]
    public void GetPrompt_LoadsMarkdownPromptFiles()
    {
        FilePromptProvider provider = new(Path.Combine(AppContext.BaseDirectory, "Prompts"));

        string prompt = provider.GetPrompt("UiUxAgent");

        Assert.Contains("UI/UX Agent", prompt);
        Assert.Contains("Responsibilities", prompt);
    }
}
