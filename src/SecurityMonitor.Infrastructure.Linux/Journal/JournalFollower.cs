using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class JournalFollower : IDisposable
{
    private static readonly string[] JournalctlArguments = ["--follow", "--lines=0", "--output=json", "SYSLOG_FACILITY=10"];

    private readonly ILogger _logger;
    private readonly string _watching;
    private Process? _journalctl;
    private CancellationTokenSource? _cts;

    public JournalFollower(ILogger logger, string watching)
    {
        _logger = logger;
        _watching = watching;
    }

    public void Start(Action<string> onLine)
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
        _ = ReadLinesAsync(_journalctl, onLine, _cts.Token);
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

    private async Task ReadLinesAsync(Process journalctl, Action<string> onLine, CancellationToken ct)
    {
        try
        {
            while (await journalctl.StandardOutput.ReadLineAsync(ct) is { } line)
                HandleLine(line, onLine);

            if (!ct.IsCancellationRequested)
                _logger.LogError("journalctl stopped unexpectedly. {Watching} are no longer being watched.", _watching);
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

    private void HandleLine(string line, Action<string> onLine)
    {
        try
        {
            onLine(line);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a journal entry");
        }
    }
}