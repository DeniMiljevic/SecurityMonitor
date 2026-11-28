using System.IO.Pipes;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using SecurityMonitor.Contracts.Monitor;
using SecurityMonitor.Host.Windows.Configuration;
using SecurityMonitor.Infrastructure.Windows.Security;
using StreamJsonRpc;

namespace SecurityMonitor.Host.Windows.Workers;

internal sealed class MonitorPipeListener : BackgroundService
{
    private readonly IMonitorService _service;
    private readonly string _pipeName;
    private readonly string _clientAccount;
    private readonly ILogger<MonitorPipeListener> _logger;

    public MonitorPipeListener(IMonitorService service, IOptions<ApiTokenOptions> options, ILogger<MonitorPipeListener> logger)
        : this(service, MonitorPipe.Name, options.Value.ServiceAccount, logger) { }

    internal MonitorPipeListener(IMonitorService service, string pipeName, string clientAccount, ILogger<MonitorPipeListener> logger)
    {
        _service = service;
        _pipeName = pipeName;
        _clientAccount = clientAccount;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        SecurityIdentifier? client = ResolveClient();
        if (client is null)
            return;

        _logger.LogInformation("Monitor pipe {Pipe} is open for {Account}", _pipeName, _clientAccount);

        bool firstInstance = true;

        while (!ct.IsCancellationRequested)
        {
            NamedPipeServerStream? pipe = await AcceptAsync(client, firstInstance, ct);
            if (pipe is null)
                return;

            firstInstance = false;
            _ = ServeAsync(pipe, ct);
        }
    }

    private SecurityIdentifier? ResolveClient()
    {
        try
        {
            return (SecurityIdentifier)new NTAccount(_clientAccount).Translate(typeof(SecurityIdentifier));
        }
        catch (Exception ex) when (ex is IdentityNotMappedException or ArgumentException)
        {
            _logger.LogWarning("Account {Account} does not exist, so the monitor pipe is not opened", _clientAccount);
            return null;
        }
    }

    private async Task<NamedPipeServerStream?> AcceptAsync(SecurityIdentifier client, bool firstInstance, CancellationToken ct)
    {
        NamedPipeServerStream? pipe = null;

        try
        {
            pipe = RestrictedPipe.Create(_pipeName, [client], firstInstance);
            await pipe.WaitForConnectionAsync(ct);
            return pipe;
        }
        catch (OperationCanceledException)
        {
            // The service is stopping.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Monitor pipe {Pipe} stopped accepting connections, so the API cannot reach the monitoring service", _pipeName);
        }

        if (pipe is not null)
            await pipe.DisposeAsync();

        return null;
    }

    private async Task ServeAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        await using (pipe)
        {
            try
            {
                using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
                rpc.AddLocalRpcTarget<IMonitorService>(_service, options: null);
                rpc.StartListening();

                await rpc.Completion.WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // The service is stopping.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "A connection on monitor pipe {Pipe} ended with an error", _pipeName);
            }
        }
    }
}
