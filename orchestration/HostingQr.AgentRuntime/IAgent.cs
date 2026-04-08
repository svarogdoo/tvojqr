using HostingQr.AgentContracts;

namespace HostingQr.AgentRuntime;

public interface IAgent
{
    AgentRole Role { get; }

    Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default);
}
