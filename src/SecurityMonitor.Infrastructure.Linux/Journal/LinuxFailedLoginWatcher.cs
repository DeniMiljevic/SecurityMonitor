using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class LinuxFailedLoginWatcher : IFailedLoginWatcher
{
    private static readonly string[] JournalctlArguments = ["--follow", "--lines=0", "--output=json", "SYSLOG_FACILITY=10"];

    private readonly ILogger<LinuxFailedLoginWatcher> _logger;
    private Process? _journalctl;
    private CancellationTokenSource? _cts;

    public LinuxFailedLoginWatcher(ILogger<LinuxFailedLoginWatcher> logger) =>
        _logger = logger;

    public event EventHandler<FailedLoginEvent>? FailedLoginDetected;

    public void Start()
    {
        if (_journalctl is not null)
            return;

        if (!Environment.IsPrivilegedProcess)
            _logger.LogWarning("Not running as root: failed logins on the login screen will not be visible. Run the app with sudo.");

        var startInfo = new ProcessStartInfo("journalctl")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (string argument in JournalctlArguments)
            startInfo.ArgumentList.Add(argument);

        _journalctl = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start journalctl.");
        _cts = new CancellationTokenSource();
        _ = ReadEntriesAsync(_journalctl, _cts.Token);

        _logger.LogInformation("Watching the system journal for failed logins (PAM authentication failures)");
    }

    public void Stop()
    {
        if (_journalctl is null)
            return;

        _cts?.Cancel();

        try
        {
            if (!_journalctl.HasExited)
                _journalctl.Kill();
        }
        catch (InvalidOperationException)
        {
            // The process already exited.
        }

        _journalctl.Dispose();
        _journalctl = null;
        _cts?.Dispose();
        _cts = null;

        _logger.LogInformation("Stopped watching the system journal");
    }

    public void Dispose() => Stop();

    private async Task ReadEntriesAsync(Process journalctl, CancellationToken ct)
    {
        try
        {
            while (await journalctl.StandardOutput.ReadLineAsync(ct) is { } line)
            {
                LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(line);

                if (loginEvent is not null)
                    Raise(loginEvent);
            }

            if (!ct.IsCancellationRequested)
                _logger.LogError("journalctl stopped unexpectedly. Failed logins are no longer being watched.");
        }
        catch (OperationCanceledException)
        {
            // Stop() was called.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed while reading the system journal");
        }
    }

    private void Raise(LinuxFailedLoginEvent loginEvent)
    {
        try
        {
            FailedLoginDetected?.Invoke(this, loginEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a journal entry");
        }
    }
}
