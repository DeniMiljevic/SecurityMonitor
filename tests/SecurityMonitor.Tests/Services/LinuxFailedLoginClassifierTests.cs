using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Services;

public class LinuxFailedLoginClassifierTests
{
    private readonly LinuxFailedLoginClassifier _classifier = new();

    [Theory]
    [InlineData("lightdm")]
    [InlineData("cinnamon-screensaver")]
    public void IsPhysicalAttempt_LoginOrLockScreen_ReturnsTrue(string service)
    {
        FailedLoginEvent loginEvent = TestEvents.LinuxFailure(service);

        Assert.True(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Theory]
    [InlineData("sudo")]
    [InlineData("sshd")]
    [InlineData("su")]
    public void IsPhysicalAttempt_OtherService_ReturnsFalse(string service)
    {
        FailedLoginEvent loginEvent = TestEvents.LinuxFailure(service);

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Fact]
    public void IsPhysicalAttempt_RemoteHost_ReturnsFalse()
    {
        FailedLoginEvent loginEvent = TestEvents.LinuxFailure() with { SourceNetworkAddress = "192.168.1.50" };

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Fact]
    public void IsPhysicalAttempt_WindowsEvent_ReturnsFalse()
    {
        FailedLoginEvent loginEvent = TestEvents.PinFailure();

        Assert.False(_classifier.IsPhysicalAttempt(loginEvent));
    }

    [Fact]
    public void GetAuthenticationMethod_LinuxEvent_ReturnsPassword()
    {
        FailedLoginEvent loginEvent = TestEvents.LinuxFailure();

        Assert.Equal(AuthenticationMethod.Password, _classifier.GetAuthenticationMethod(loginEvent));
    }
}