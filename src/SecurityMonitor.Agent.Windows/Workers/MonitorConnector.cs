using System.IO.Pipes;
using SecurityMonitor.Contracts.Agent;
using StreamJsonRpc;

namespace SecurityMonitor.Agent.Windows.Workers;

internal sealed class MonitorConnector : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan AccessDeniedRetryDelay = TimeSpan.FromMinutes(1);
    private readonly IAgentService _agent;
    private readonly string _pipeName;
    private readonly ILogger<MonitorConnector> _logger;

    public MonitorConnector(IAgentService agent, IConfiguration config, ILogger<MonitorConnector> logger)
    {
        _agent = agent;
        _pipeName = config["Agent:PipeName"] ?? AgentPipe.Name;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TimeSpan delay = RetryDelay;

            try
            {
                await ServeOneConnectionAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("This account may not connect to the monitoring service. Add it to Agent:Users on this PC");
                delay = AccessDeniedRetryDelay;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("The connection to the monitoring service ended: {Error}", ex.Message);
            }

            try
            {
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task ServeOneConnectionAsync(CancellationToken ct)
    {
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        _logger.LogInformation("Waiting for the monitoring service");
        await pipe.ConnectAsync(ct);
        _logger.LogInformation("Connected to the monitoring service");

        using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
        rpc.AddLocalRpcTarget<IAgentService>(_agent, options: null);
        rpc.StartListening();

        await rpc.Completion.WaitAsync(ct);
        _logger.LogWarning("The monitoring service closed the connection");
    }
}
