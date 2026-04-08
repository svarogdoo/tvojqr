using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public interface IRunStore
{
    Task<string> CreateRunDirectoryAsync(Guid runId, DateTimeOffset startedAt, CancellationToken cancellationToken = default);

    Task SaveManifestAsync(RunManifest manifest, CancellationToken cancellationToken = default);

    Task SaveResultAsync(OrchestrationResult result, CancellationToken cancellationToken = default);
}
