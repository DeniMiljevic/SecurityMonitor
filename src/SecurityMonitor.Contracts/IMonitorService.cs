namespace SecurityMonitor.Contracts;

public interface IMonitorService
{
    Task<MonitorStatus> GetStatusAsync(CancellationToken ct);
}
