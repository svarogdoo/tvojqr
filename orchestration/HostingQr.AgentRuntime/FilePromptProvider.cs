namespace HostingQr.AgentRuntime;

public sealed class FilePromptProvider : IPromptProvider
{
    private readonly string _promptRoot;

    public FilePromptProvider(string promptRoot)
    {
        _promptRoot = promptRoot;
    }

    public string GetPrompt(string promptName)
    {
        string path = Path.Combine(_promptRoot, $"{promptName}.md");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Prompt file not found for '{promptName}'.", path);
        }

        return File.ReadAllText(path);
    }
}
