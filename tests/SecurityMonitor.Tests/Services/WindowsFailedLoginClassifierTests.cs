using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Services;

public class WindowsFailedLoginClassifierTests
{
    private readonly WindowsFailedLoginClassifier _classifier = new();

    [Fact]
    public void IsPhysicalAttempt_WrongPinAtLockScreen_ReturnsTrue()
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure();

        bool result = _classifier.IsPhysicalAttempt(loginEvent);

        Assert.True(result);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(11)]
    public void IsPhysicalAttempt_ConsoleLogonType_ReturnsTrue(int logonType)
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure() with { LogonType = logonType };

        Assert.True(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(8)]
    [InlineData(10)]
    public void IsPhysicalAttempt_NonConsoleLogonType_ReturnsFalse(int logonType)
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure() with { LogonType = logonType };

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Fact]
    public void IsPhysicalAttempt_RemoteSourceAddress_ReturnsFalse()
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure() with { SourceNetworkAddress = "192.168.1.50" };

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Fact]
    public void IsPhysicalAttempt_EventIdNot4625_ReturnsFalse()
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure() with { EventId = 4624 };

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Theory]
    [InlineData(0xC0000380u, AuthenticationMethod.Pin)]
    [InlineData(0xC000006Au, AuthenticationMethod.Password)]
    [InlineData(0xC0000064u, AuthenticationMethod.Unknown)]
    public void GetAuthenticationMethod_SubStatus_ReturnsMatchingMethod(uint subStatus, AuthenticationMethod expected)
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure() with { FailureSubStatus = subStatus };

        Assert.Equal(expected, _classifier.GetAuthenticationMethod(loginEvent));
    }
}
