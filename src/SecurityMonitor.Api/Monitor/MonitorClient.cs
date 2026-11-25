using System.IO.Pipes;
using SecurityMonitor.Contracts;
using StreamJsonRpc;

namespace SecurityMonitor.Api.Monitor;

public sealed class MonitorClient : IMonitorService
{
    private const int ConnectTimeoutMs = 2000;

    private readonly string _pipeName;

    public MonitorClient(IConfiguration config) => _pipeName = config["Monitor:PipeName"] ?? MonitorPipe.Name;

    public Task<MonitorStatus> GetStatusAsync(CancellationToken ct) =>
        CallAsync(monitor => monitor.GetStatusAsync(ct), ct);

    public Task<IReadOnlyList<IncidentSummary>> GetIncidentsAsync(int take, CancellationToken ct) =>
        CallAsync(monitor => monitor.GetIncidentsAsync(take, ct), ct);

    private async Task<T> CallAsync<T>(Func<IMonitorService, Task<T>> call, CancellationToken ct)
    {
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ConnectTimeoutMs, ct);

        using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
        IMonitorService monitor = rpc.Attach<IMonitorService>();
        rpc.StartListening();

        return await call(monitor);
    }
}
