using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class LinuxSuccessfulLoginWatcher : ISuccessfulLoginWatcher
{
    private readonly ILogger<LinuxSuccessfulLoginWatcher> _logger;
    private readonly JournalFollower _journal;

    public LinuxSuccessfulLoginWatcher(ILogger<LinuxSuccessfulLoginWatcher> logger)
    {
        _logger = logger;
        _journal = new JournalFollower(logger, "Successful logins");
    }

    public event EventHandler<SuccessfulLoginEvent>? SuccessfulLoginDetected;

    public void Start()
    {
        _journal.Start(OnLine);
        _logger.LogInformation("Watching the system journal for successful logins (PAM sessions)");
    }

    public void Stop() => _journal.Stop();

    public void Dispose() => _journal.Dispose();

    private void OnLine(string line)
    {
        SuccessfulLoginEvent? loginEvent = JournalEntryParser.TryParseSessionOpened(line);

        if (loginEvent is not null)
            SuccessfulLoginDetected?.Invoke(this, loginEvent);
    }
}