using SecurityMonitor.Contracts;

namespace SecurityMonitor.Tests.Api.Fakes;
internal sealed class FakeMonitorService : IMonitorService
{
    public MonitorStatus? Status { get; set; } = new("1.0.0.0", DateTimeOffset.UnixEpoch, IncidentCount: 0);

    public List<IncidentSummary> Incidents { get; } = [];

    public int? RequestedTake { get; private set; }

    public Task<MonitorStatus> GetStatusAsync(CancellationToken ct) =>
        Status is null ? Unreachable<MonitorStatus>() : Task.FromResult(Status);

    public Task<IReadOnlyList<IncidentSummary>> GetIncidentsAsync(int take, CancellationToken ct)
    {
        RequestedTake = take;

        return Status is null
            ? Unreachable<IReadOnlyList<IncidentSummary>>()
            : Task.FromResult<IReadOnlyList<IncidentSummary>>(Incidents.Take(take).ToList());
    }

    private static Task<T> Unreachable<T>() =>
        Task.FromException<T>(new TimeoutException("The monitoring service is not running"));
}