using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class LinuxFailedLoginWatcher : IFailedLoginWatcher
{
    private readonly ILogger<LinuxFailedLoginWatcher> _logger;
    private readonly JournalFollower _journal;

    public LinuxFailedLoginWatcher(ILogger<LinuxFailedLoginWatcher> logger)
    {
        _logger = logger;
        _journal = new JournalFollower(logger, "Failed logins");
    }

    public event EventHandler<FailedLoginEvent>? FailedLoginDetected;

    public void Start()
    {
        if (!Environment.IsPrivilegedProcess)
            _logger.LogWarning("Not running as root: failed logins on the login screen will not be visible. Run the app with sudo.");

        _journal.Start(OnLine);
        _logger.LogInformation("Watching the system journal for failed logins (PAM authentication failures)");
    }

    public void Stop()
    {
        _journal.Stop();
        _logger.LogInformation("Stopped watching the system journal");
    }

    public void Dispose() => _journal.Dispose();

    private void OnLine(string line)
    {
        LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(line);

        if (loginEvent is not null)
            FailedLoginDetected?.Invoke(this, loginEvent);
    }
}