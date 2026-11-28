using System.Diagnostics;
using SecurityMonitor.Contracts.Monitor;
using SecurityMonitor.Infrastructure.Data.Queries;

namespace SecurityMonitor.Host.Windows.Pipe;

internal sealed class MonitorService : IMonitorService
{
    private static readonly DateTimeOffset StartedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();
    private readonly IncidentReader _incidents;

    public MonitorService(IncidentReader incidents) => _incidents = incidents;

    public async Task<MonitorStatus> GetStatusAsync(CancellationToken ct) => new(
        Version: typeof(MonitorService).Assembly.GetName().Version?.ToString() ?? "unknown",
        StartedAtUtc: StartedAtUtc,
        IncidentCount: await _incidents.CountAsync(ct));

    public Task<IReadOnlyList<IncidentSummary>> GetIncidentsAsync(int take, CancellationToken ct) =>
        _incidents.GetRecentAsync(take, ct);
}
