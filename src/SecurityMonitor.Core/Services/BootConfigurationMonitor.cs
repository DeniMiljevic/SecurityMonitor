using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;

namespace SecurityMonitor.Core.Services;

public sealed class BootConfigurationMonitor
{
    private const int MaxAttempts = 10;

    private readonly IBootConfigurationCheck _check;
    private readonly INotifier _notifier;
    private readonly ILogger<BootConfigurationMonitor> _logger;

    public BootConfigurationMonitor(IBootConfigurationCheck check, INotifier notifier, ILogger<BootConfigurationMonitor> logger)
    {
        _check = check;
        _notifier = notifier;
        _logger = logger;
    }

    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(30);

    public async Task RunAsync(CancellationToken ct = default)
    {
        IReadOnlyList<string> problems;

        try
        {
            problems = await _check.FindProblemsAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            problems = [$"Could not check the boot configuration: {ex.Message}"];
        }

        if (problems.Count == 0)
        {
            _logger.LogInformation("Boot configuration OK: Linux is first and the Windows entry exists");
            return;
        }

        foreach (string problem in problems)
            _logger.LogWarning("Boot configuration problem: {Problem}", problem);

        await NotifyWithRetryAsync(string.Join("\n", problems.Select(p => "• " + p)), ct);
    }

    private async Task NotifyWithRetryAsync(string message, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await _notifier.NotifyWarningAsync(message, ct);
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not send the boot configuration warning (attempt {Attempt}/{Max}): {Error}", attempt, MaxAttempts, ex.Message);
            }

            if (attempt < MaxAttempts)
                await Task.Delay(RetryDelay, ct);
        }

        _logger.LogError("Gave up sending the boot configuration warning");
    }
}
