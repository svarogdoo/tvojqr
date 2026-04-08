namespace HostingQr.AgentRuntime;

public static class PromptClassifier
{
    public static bool LooksLikeUiWork(string prompt) => ContainsAny(prompt, "ui", "ux", "design", "layout", "page", "landing", "hero", "button", "form");

    public static bool LooksLikeArchitectureWork(string prompt) => ContainsAny(prompt, "architect", "architecture", "backend", "database", "storage", "auth", "workflow", "orchestration");

    public static bool LooksLikeImplementationWork(string prompt) => ContainsAny(prompt, "add", "build", "implement", "fix", "create", "wire", "setup");

    private static bool ContainsAny(string prompt, params string[] tokens)
    {
        foreach (string token in tokens)
        {
            if (prompt.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
