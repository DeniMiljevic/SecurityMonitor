namespace SecurityMonitor.Contracts.Agent;

public interface IAgentService
{
    const int MaxPhotoBytes = 5 * 1024 * 1024;

    Task<AgentPhoto> CapturePhotoAsync(CancellationToken ct);
}
