using HostingQr.AgentContracts;

namespace HostingQr.Tools;

public interface IRepoInspector
{
    Task<RepositoryContext> BuildContextAsync(string repositoryRoot, string prompt, CancellationToken cancellationToken = default);
}
