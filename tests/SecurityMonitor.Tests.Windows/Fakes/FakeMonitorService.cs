using SecurityMonitor.Contracts.Monitor;

namespace SecurityMonitor.Tests.Windows.Fakes;

internal sealed class FakeMonitorService : IMonitorService
{
    private readonly MonitorStatus _status;
    private readonly IReadOnlyList<IncidentSummary> _incidents;

    public FakeMonitorService(MonitorStatus status, IReadOnlyList<IncidentSummary> incidents)
    {
        _status = status;
        _incidents = incidents;
    }

    public Task<MonitorStatus> GetStatusAsync(CancellationToken ct) => Task.FromResult(_status);

    public Task<IReadOnlyList<IncidentSummary>> GetIncidentsAsync(int take, CancellationToken ct) => Task.FromResult(_incidents);
}