namespace SecurityMonitor.Contracts.Monitor;

public interface IMonitorService
{
    const int MaxIncidents = 200;

    Task<MonitorStatus> GetStatusAsync(CancellationToken ct);

    Task<IReadOnlyList<IncidentSummary>> GetIncidentsAsync(int take, CancellationToken ct);
}
