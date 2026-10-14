using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Infrastructure.Common.Hosting;

public sealed class Worker : BackgroundService
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
            // The host is shutting down.
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
            // The host is shutting down.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while handling a failed login");
        }
    }
}