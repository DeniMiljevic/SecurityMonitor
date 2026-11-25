using SecurityMonitor.Contracts;

namespace SecurityMonitor.Tests.Api.Fakes;

internal sealed class FakeMonitorService : IMonitorService
{
    public MonitorStatus? Status { get; set; } = new("1.0.0.0", DateTimeOffset.UnixEpoch, IncidentCount: 0);

    public Task<MonitorStatus> GetStatusAsync(CancellationToken ct) =>
        Status is null ? Task.FromException<MonitorStatus>(new TimeoutException("The monitoring service is not running")) : Task.FromResult(Status);
}
