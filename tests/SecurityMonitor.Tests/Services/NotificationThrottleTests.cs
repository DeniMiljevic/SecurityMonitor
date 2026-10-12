using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Tests.Services;

public class NotificationThrottleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 12, 20, 0, 0, TimeSpan.FromHours(2));

    [Fact]
    public void ShouldNotify_FirstCall_ReturnsTrue()
    {
        var throttle = new NotificationThrottle(TimeSpan.FromMinutes(2));

        Assert.True(throttle.ShouldNotify(Start));
    }

    [Fact]
    public void ShouldNotify_SecondCallInsideCooldown_ReturnsFalse()
    {
        var throttle = new NotificationThrottle(TimeSpan.FromMinutes(2));

        throttle.ShouldNotify(Start);
        bool second = throttle.ShouldNotify(Start.AddMinutes(1));

        Assert.False(second);
    }

    [Fact]
    public void ShouldNotify_AfterCooldown_ReturnsTrue()
    {
        var throttle = new NotificationThrottle(TimeSpan.FromMinutes(2));

        throttle.ShouldNotify(Start);
        bool later = throttle.ShouldNotify(Start.AddMinutes(3));

        Assert.True(later);
    }
}
