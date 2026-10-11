using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Services;

public class FailedLoginPolicyTests
{
    private static FailedLoginPolicy CreatePolicy(int attempts = 3, int windowMinutes = 5, int cooldownMinutes = 0) =>
        new(new SecurityMonitorOptions
        {
            FailedAttemptsBeforeCapture = attempts,
            AttemptWindowMinutes = windowMinutes,
            CaptureCooldownMinutes = cooldownMinutes
        });

    [Fact]
    public void Evaluate_OneAttemptWithThresholdThree_DoesNotCapture()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 3);

        PolicyDecision decision = policy.Evaluate(TestEvents.AtMinute(0));

        Assert.False(decision.ShouldCapture);
        Assert.Equal(1, decision.AttemptsInWindow);
    }

    [Fact]
    public void Evaluate_ThreeAttemptsInsideWindow_Captures()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 3, windowMinutes: 5);

        policy.Evaluate(TestEvents.AtMinute(0));
        policy.Evaluate(TestEvents.AtMinute(0.25));
        PolicyDecision third = policy.Evaluate(TestEvents.AtMinute(0.5));

        Assert.True(third.ShouldCapture);
        Assert.Equal(3, third.AttemptsInWindow);
    }

    [Fact]
    public void Evaluate_AttemptsOutsideWindow_DoesNotCapture()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 3, windowMinutes: 5);

        PolicyDecision first = policy.Evaluate(TestEvents.AtMinute(0));
        PolicyDecision second = policy.Evaluate(TestEvents.AtMinute(6));
        PolicyDecision third = policy.Evaluate(TestEvents.AtMinute(12));

        Assert.False(first.ShouldCapture);
        Assert.False(second.ShouldCapture);
        Assert.False(third.ShouldCapture);
        Assert.Equal(1, third.AttemptsInWindow);
    }

    [Fact]
    public void Evaluate_ThresholdOneWithoutCooldown_CapturesEveryAttempt()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 1, windowMinutes: 5, cooldownMinutes: 0);

        Assert.True(policy.Evaluate(TestEvents.AtMinute(0)).ShouldCapture);
        Assert.True(policy.Evaluate(TestEvents.AtMinute(0.1)).ShouldCapture);
        Assert.True(policy.Evaluate(TestEvents.AtMinute(0.2)).ShouldCapture);
    }

    [Fact]
    public void Evaluate_AttemptDuringCooldown_DoesNotCapture()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 1, cooldownMinutes: 2);

        PolicyDecision atZero = policy.Evaluate(TestEvents.AtMinute(0));
        PolicyDecision atOne = policy.Evaluate(TestEvents.AtMinute(1));
        PolicyDecision atThree = policy.Evaluate(TestEvents.AtMinute(3));

        Assert.True(atZero.ShouldCapture);
        Assert.False(atOne.ShouldCapture);
        Assert.True(atOne.InCooldown);
        Assert.True(atThree.ShouldCapture);
    }

    [Fact]
    public void Evaluate_AttemptAfterCapture_StartsCountingFromOne()
    {
        FailedLoginPolicy policy = CreatePolicy(attempts: 3);

        policy.Evaluate(TestEvents.AtMinute(0));
        policy.Evaluate(TestEvents.AtMinute(0.1));
        policy.Evaluate(TestEvents.AtMinute(0.2));
        PolicyDecision next = policy.Evaluate(TestEvents.AtMinute(0.3));

        Assert.False(next.ShouldCapture);
        Assert.Equal(1, next.AttemptsInWindow);
    }

    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 5, -1)]
    public void Constructor_InvalidSettings_ThrowsArgumentOutOfRange(int attempts, int windowMinutes, int cooldownMinutes) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePolicy(attempts, windowMinutes, cooldownMinutes));
}