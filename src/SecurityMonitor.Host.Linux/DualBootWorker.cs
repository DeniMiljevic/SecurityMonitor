using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Host.Linux;

public sealed class DualBootWorker : BackgroundService
{
    private readonly ISuccessfulLoginWatcher _watcher;
    private readonly DualBootMonitor _monitor;
    private readonly ILogger<DualBootWorker> _logger;
    private CancellationToken _ct;

    public DualBootWorker(ISuccessfulLoginWatcher watcher, DualBootMonitor monitor, ILogger<DualBootWorker> logger)
    {
        _watcher = watcher;
        _monitor = monitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _ct = ct;

        _watcher.SuccessfulLoginDetected += OnSuccessfulLoginDetected;
        _watcher.Start();

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // The host is shutting down.
        }
        finally
        {
            _watcher.SuccessfulLoginDetected -= OnSuccessfulLoginDetected;
            _watcher.Stop();
        }
    }

    private void OnSuccessfulLoginDetected(object? sender, SuccessfulLoginEvent e)
    {
        _ = HandleSafelyAsync(e);
    }

    private async Task HandleSafelyAsync(SuccessfulLoginEvent e)
    {
        try
        {
            await _monitor.HandleAsync(e, _ct);
        }
        catch (OperationCanceledException)
        {
            // The host is shutting down.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while handling a successful login");
        }
    }
}
