using HostingQr.AgentRuntime;

namespace HostingQr.Tests;

public sealed class PromptClassifierTests
{
    [Theory]
    [InlineData("Improve the hero layout", true)]
    [InlineData("Design the pricing section", true)]
    [InlineData("Set up PostgreSQL storage", false)]
    public void LooksLikeUiWork_RecognizesUiPrompts(string prompt, bool expected)
    {
        Assert.Equal(expected, PromptClassifier.LooksLikeUiWork(prompt));
    }

    [Theory]
    [InlineData("Set up orchestration backend", true)]
    [InlineData("Add auth and storage", true)]
    [InlineData("Polish the footer design", false)]
    public void LooksLikeArchitectureWork_RecognizesArchitecturePrompts(string prompt, bool expected)
    {
        Assert.Equal(expected, PromptClassifier.LooksLikeArchitectureWork(prompt));
    }
}
