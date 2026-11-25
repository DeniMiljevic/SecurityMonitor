using System.IO.Pipes;
using SecurityMonitor.Contracts;
using StreamJsonRpc;

namespace SecurityMonitor.Api.Monitor;

public sealed class MonitorClient : IMonitorService
{
    private const int ConnectTimeoutMs = 2000;

    private readonly string _pipeName;

    public MonitorClient(IConfiguration config) => _pipeName = config["Monitor:PipeName"] ?? MonitorPipe.Name;

    public async Task<MonitorStatus> GetStatusAsync(CancellationToken ct)
    {
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ConnectTimeoutMs, ct);

        using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
        IMonitorService monitor = rpc.Attach<IMonitorService>();
        rpc.StartListening();

        return await monitor.GetStatusAsync(ct);
    }
}
