using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class FailedLoginPolicy
{
    private readonly int _threshold;
    private readonly TimeSpan _window;
    private readonly TimeSpan _cooldown;
    private readonly Queue<DateTimeOffset> _attempts = new();
    private readonly Lock _lock = new();
    private DateTimeOffset? _lastCaptureAt;

    public FailedLoginPolicy(SecurityMonitorOptions options)
    {
        if (options.FailedAttemptsBeforeCapture < 1)
            throw new ArgumentOutOfRangeException(nameof(options), "FailedAttemptsBeforeCapture must be at least 1.");

        if (options.AttemptWindowMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(options), "AttemptWindowMinutes must be at least 1.");

        if (options.CaptureCooldownMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "CaptureCooldownMinutes cannot be negative.");

        _threshold = options.FailedAttemptsBeforeCapture;
        _window = TimeSpan.FromMinutes(options.AttemptWindowMinutes);
        _cooldown = TimeSpan.FromMinutes(options.CaptureCooldownMinutes);
    }

    public PolicyDecision Evaluate(FailedLoginEvent loginEvent)
    {
        lock (_lock)
        {
            DateTimeOffset time = loginEvent.Timestamp;

            RemoveExpiredAttempts(time);
            _attempts.Enqueue(time);

            int attempts = _attempts.Count;
            bool inCooldown = _lastCaptureAt is DateTimeOffset last && time - last < _cooldown;
            bool shouldCapture = attempts >= _threshold && !inCooldown;

            if (shouldCapture)
            {
                _lastCaptureAt = time;
                _attempts.Clear();
            }

            return new PolicyDecision(shouldCapture, attempts, _threshold, inCooldown);
        }
    }

    private void RemoveExpiredAttempts(DateTimeOffset time)
    {
        while (_attempts.Count > 0 && time - _attempts.Peek() > _window)
            _attempts.Dequeue();
    }
}
