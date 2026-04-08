using System.Diagnostics;
using System.Text;
using HostingQr.AgentContracts;

namespace HostingQr.Tools;

public sealed class FileSystemRepoInspector : IRepoInspector
{
    private static readonly string[] SourceExtensions = [".svelte", ".ts", ".js", ".cs"];

    public async Task<RepositoryContext> BuildContextAsync(string repositoryRoot, string prompt, CancellationToken cancellationToken = default)
    {
        string readmePath = Path.Combine(repositoryRoot, "README.md");
        string agentsPath = Path.Combine(repositoryRoot, "AGENTS.md");

        string readme = File.Exists(readmePath)
            ? SummarizeLines(await File.ReadAllLinesAsync(readmePath, cancellationToken), 80)
            : "README.md not found.";

        string agents = File.Exists(agentsPath)
            ? SummarizeLines(await File.ReadAllLinesAsync(agentsPath, cancellationToken), 80)
            : "AGENTS.md not found.";

        string gitStatus = await RunGitCommandAsync(repositoryRoot, "status --short --branch", cancellationToken);
        string recentCommits = await RunGitCommandAsync(repositoryRoot, "log --oneline --max-count=5", cancellationToken);
        IReadOnlyList<string> relevantFiles = GetRelevantFiles(repositoryRoot, prompt);

        return new RepositoryContext(
            repositoryRoot,
            readme,
            agents,
            gitStatus.Trim(),
            recentCommits.Trim(),
            relevantFiles);
    }

    private static IReadOnlyList<string> GetRelevantFiles(string repositoryRoot, string prompt)
    {
        string srcRoot = Path.Combine(repositoryRoot, "src");
        if (!Directory.Exists(srcRoot))
        {
            return [];
        }

        HashSet<string> tokens = prompt
            .Split([' ', ',', '.', ':', ';', '-', '_', '/', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 3)
            .Select(token => token.ToLowerInvariant())
            .ToHashSet();

        return Directory
            .EnumerateFiles(srcRoot, "*", SearchOption.AllDirectories)
            .Where(path => SourceExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Select(path => new
            {
                Relative = Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'),
                Score = Score(path, tokens),
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Relative, StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .Select(item => item.Relative)
            .ToArray();
    }

    private static int Score(string path, HashSet<string> tokens)
    {
        string lowered = path.ToLowerInvariant();
        int score = 0;
        foreach (string token in tokens)
        {
            if (lowered.Contains(token, StringComparison.Ordinal))
            {
                score += 2;
            }
        }

        if (lowered.Contains("routes", StringComparison.Ordinal))
        {
            score += 1;
        }

        return score;
    }

    private static string SummarizeLines(IReadOnlyList<string> lines, int maxLines)
    {
        return string.Join(Environment.NewLine, lines.Take(maxLines));
    }

    private static async Task<string> RunGitCommandAsync(string repositoryRoot, string arguments, CancellationToken cancellationToken)
    {
        try
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            string stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                return $"git {arguments} failed: {stderr.Trim()}";
            }

            return stdout;
        }
        catch (Exception ex)
        {
            return $"git {arguments} failed: {ex.Message}";
        }
    }
}
