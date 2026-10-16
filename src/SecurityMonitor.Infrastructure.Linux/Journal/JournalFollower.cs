using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public sealed class JournalFollower : IDisposable
{
    private static readonly string[] JournalctlArguments = ["--follow", "--lines=0", "--output=json", "SYSLOG_FACILITY=10"];
    private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(5);

    private readonly ILogger _logger;
    private readonly string _watching;
    private readonly Lock _lock = new();
    private Process? _journalctl;
    private CancellationTokenSource? _cts;

    public JournalFollower(ILogger logger, string watching)
    {
        _logger = logger;
        _watching = watching;
    }

    public void Start(Action<string> onLine)
    {
        lock (_lock)
        {
            if (_cts is not null)
                return;

            _cts = new CancellationTokenSource();
            _ = RunAsync(onLine, _cts.Token);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_cts is null)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            KillCurrentProcess();
        }
    }

    public void Dispose() => Stop();

    private async Task RunAsync(Action<string> onLine, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using Process journalctl = StartJournalctl(ct);
                await ReadLinesAsync(journalctl, onLine, ct);

                if (ct.IsCancellationRequested)
                    return;

                _logger.LogWarning("journalctl stopped unexpectedly. Restarting it in {Delay} s so {Watching} keep being watched.", RestartDelay.TotalSeconds, _watching);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while watching the system journal for {Watching}. Retrying in {Delay} s.", _watching, RestartDelay.TotalSeconds);
            }

            try
            {
                await Task.Delay(RestartDelay, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private Process StartJournalctl(CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo("journalctl")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (string argument in JournalctlArguments)
            startInfo.ArgumentList.Add(argument);

        lock (_lock)
        {
            ct.ThrowIfCancellationRequested();

            _journalctl = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start journalctl.");
            return _journalctl;
        }
    }

    private async Task ReadLinesAsync(Process journalctl, Action<string> onLine, CancellationToken ct)
    {
        try
        {
            while (await journalctl.StandardOutput.ReadLineAsync(ct) is { } line)
                HandleLine(line, onLine);
        }
        finally
        {
            lock (_lock)
            {
                if (ReferenceEquals(_journalctl, journalctl))
                    _journalctl = null;
            }
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

    private void KillCurrentProcess()
    {
        try
        {
            if (_journalctl is { HasExited: false })
                _journalctl.Kill();
        }
        catch (InvalidOperationException)
        {
            // The process already exited.
        }

        _journalctl = null;
    }
}