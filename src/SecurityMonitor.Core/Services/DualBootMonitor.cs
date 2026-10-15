using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class DualBootMonitor
{
    private readonly DualBootPolicy _policy;
    private readonly IBootSwitcher _switcher;
    private readonly ILogger<DualBootMonitor> _logger;
    private int _switchInProgress;

    public DualBootMonitor(DualBootPolicy policy, IBootSwitcher switcher, ILogger<DualBootMonitor> logger)
    {
        _policy = policy;
        _switcher = switcher;
        _logger = logger;
    }

    public async Task HandleAsync(SuccessfulLoginEvent loginEvent, CancellationToken ct = default)
    {
        DualBootAction action = _policy.Evaluate(loginEvent);

        if (action == DualBootAction.None)
            return;

        if (action == DualBootAction.StayInLinux)
        {
            _logger.LogInformation("Login as {UserName}: staying in Linux", loginEvent.UserName);
            return;
        }

        if (Interlocked.Exchange(ref  _switchInProgress, 1) == 1)
        {
            _logger.LogInformation("Login as {UserName}: a switch to Windows is already in progress", loginEvent.UserName);
            return;
        }

        bool rebooting = false;
        try
        {
            _logger.LogWarning("Login as {UserName}: switching to Windows", loginEvent.UserName);
            BootSwitchResult result = await _switcher.SwitchToWindowsAsync(ct);

            if (!result.Success)
                _logger.LogError("Could not switch to Windows: {Reason}. Staying in Linux.", result.FailureReason);
            else if (result.DryRun)
                _logger.LogInformation("DryRun is on, so the PC was not restarted");
            else
                rebooting = true;
        }
        finally
        {
            if (!rebooting)
                Interlocked.Exchange(ref _switchInProgress, 0);
        }
    }
}
