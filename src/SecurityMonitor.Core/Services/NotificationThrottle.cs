using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Core.Services;

public sealed class NotificationThrottle
{
    private readonly TimeSpan _cooldown;
    private readonly Lock _lock = new();
    private DateTimeOffset? _lastNotifiedAt;

    public NotificationThrottle(TimeSpan cooldown) => _cooldown = cooldown;

    public bool ShouldNotify(DateTimeOffset now)
    {
        lock (_lock)
        {
            if (_lastNotifiedAt is DateTimeOffset last && now - last < _cooldown)
                return false;

            _lastNotifiedAt = now;
            return true;
        }
    }
}
