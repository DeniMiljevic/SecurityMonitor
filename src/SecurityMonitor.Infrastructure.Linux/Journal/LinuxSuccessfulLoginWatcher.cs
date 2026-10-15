using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class LinuxSuccessfulLoginWatcher : ISuccessfulLoginWatcher
{
    private static readonly string[] JournalctlArguments = ["--follow", "--lines=0", "--output=json", "SYSLOG_FACILITY=10"];

    private readonly ILogger<LinuxSuccessfulLoginWatcher> _logger;
    private Process? _journalctl;
    private CancellationTokenSource? _cts;

    public LinuxSuccessfulLoginWatcher(ILogger<LinuxSuccessfulLoginWatcher> logger) =>
        _logger = logger;

    public event EventHandler<SuccessfulLoginEvent>? SuccessfulLoginDetected;

    public void Start()
    {
        if (_journalctl is not null)
            return;

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

        _logger.LogInformation("Watching the system journal for successful logins (PAM sessions)");
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
    }

    public void Dispose() => Stop();

    private async Task ReadEntriesAsync(Process journalctl, CancellationToken ct)
    {
        try
        {
            while (await journalctl.StandardOutput.ReadLineAsync(ct) is { } line)
            {
                SuccessfulLoginEvent? loginEvent = JournalEntryParser.TryParseSessionOpened(line);

                if (loginEvent is not null)
                    Raise(loginEvent);
            }

            if (!ct.IsCancellationRequested)
                _logger.LogError("journalctl stopped unexpectedly. Successful logins are no longer being watched.");
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

    private void Raise(SuccessfulLoginEvent loginEvent)
    {
        try
        {
            SuccessfulLoginDetected?.Invoke(this, loginEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a journal entry");
        }
    }
}