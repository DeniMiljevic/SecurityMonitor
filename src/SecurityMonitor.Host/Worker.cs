using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Host;

public class Worker : BackgroundService
{
    private readonly IFailedLoginWatcher _watcher;
    private readonly FailedLoginMonitor _monitor;
    private readonly ILogger<Worker> _logger;
    private CancellationToken _ct;

    public Worker(IFailedLoginWatcher watcher, FailedLoginMonitor monitor, ILogger<Worker> logger)
    {
        _watcher = watcher;
        _monitor = monitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _ct = ct;

        _watcher.FailedLoginDetected += OnFailedLoginDetected;
        _watcher.Start();

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            _watcher.FailedLoginDetected -= OnFailedLoginDetected;
            _watcher.Stop();
        }
    }

    private void OnFailedLoginDetected(object? sender, FailedLoginEvent e)
    {
        _ = HandleSafelyAsync(e);
    }

    private async Task HandleSafelyAsync(FailedLoginEvent e)
    {
        try
        {
            await _monitor.HandleAsync(e, _ct);
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while handling a failed login");
        }
    }
}
