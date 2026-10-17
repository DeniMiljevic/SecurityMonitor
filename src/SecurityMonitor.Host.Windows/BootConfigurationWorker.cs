using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Host.Windows;

public sealed class BootConfigurationWorker : BackgroundService
{
    private readonly BootConfigurationMonitor _monitor;

    public BootConfigurationWorker(BootConfigurationMonitor monitor) => _monitor = monitor;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await _monitor.RunAsync(ct);
        }
        catch (OperationCanceledException)
        {
            //The host is shutting down.
        }
    }
}
