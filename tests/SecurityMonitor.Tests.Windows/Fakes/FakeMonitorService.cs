using SecurityMonitor.Contracts;

namespace SecurityMonitor.Tests.Windows.Fakes;

internal sealed class FakeMonitorService : IMonitorService
{
    private readonly MonitorStatus _status;

    public FakeMonitorService(MonitorStatus status) => _status = status;

    public Task<MonitorStatus> GetStatusAsync(CancellationToken ct) => Task.FromResult(_status);
}