using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Host;

public class Worker : BackgroundService
{
    private readonly IFailedLoginWatcher _watcher;
    private readonly FailedLoginClassifier _classifier;
    private readonly ILogger<Worker> _logger;

    public Worker(IFailedLoginWatcher watcher, FailedLoginClassifier classifier, ILogger<Worker> logger)
    {
        _watcher = watcher;
        _classifier = classifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _watcher.FailedLoginDetected += OnFailedLoginDetected;
        _watcher.Start();

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
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
        if (!_classifier.isPhysicalAttempt(e))
        {
            _logger.LogDebug("Ignored failed logon: LogonType {LogonType}, source {Source}", e.LogonType, e.SourceNetworkAddress ?? "-");

            return;
        }

        AuthenticationMethod method = _classifier.GetAuthenticationMethod(e);

        _logger.LogInformation("Relevant failed login detected: {Method}, LogonType {LogonType}, user {UserName}", method, e.LogonType, e.UserName ?? "(unknown)");
    }
}
