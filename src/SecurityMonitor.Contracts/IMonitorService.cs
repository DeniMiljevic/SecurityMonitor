namespace SecurityMonitor.Contracts;

public interface IMonitorService
{
    const int MaxIncidents = 200;

    Task<MonitorStatus> GetStatusAsync(CancellationToken ct);
}
