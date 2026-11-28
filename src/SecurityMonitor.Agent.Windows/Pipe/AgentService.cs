using SecurityMonitor.Contracts.Agent;

namespace SecurityMonitor.Agent.Windows.Pipe;

internal sealed class AgentService : IAgentService
{
    public Task<AgentPhoto> CapturePhotoAsync(CancellationToken ct) =>
        Task.FromResult(AgentPhoto.Failed("The agent cannot take photos yet"));
}
