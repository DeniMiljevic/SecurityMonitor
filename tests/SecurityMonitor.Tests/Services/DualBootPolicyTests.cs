using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Tests.Services;

public class DualBootPolicyTests
{
    private static readonly DualBootOptions EnabledForDeni = new()
    {
        Enabled = true,
        SwitchToWindowsUsers = ["deni"]
    };

    [Fact]
    public void Evaluate_ConfiguredUserOnLoginScreen_SwitchesToWindows()
    {
        var policy = new DualBootPolicy(EnabledForDeni);

        Assert.Equal(DualBootAction.SwitchToWindows, policy.Evaluate(Login("deni", 1000)));
    }

    [Fact]
    public void Evaluate_OtherUserOnLoginScreen_StaysInLinux()
    {
        var policy = new DualBootPolicy(EnabledForDeni);

        Assert.Equal(DualBootAction.StayInLinux, policy.Evaluate(Login("admin", 1001)));
    }

    [Fact]
    public void Evaluate_Disabled_StaysInLinux()
    {
        var policy = new DualBootPolicy(new DualBootOptions { Enabled = false, SwitchToWindowsUsers = ["deni"] });

        Assert.Equal(DualBootAction.StayInLinux, policy.Evaluate(Login("deni", 1000)));
    }

    [Theory]
    [InlineData("lightdm-greeter", "lightdm", 113)]
    [InlineData("systemd-user", "deni", 1000)]
    [InlineData("sudo", "root", 0)]
    [InlineData("cron", "root", 0)]
    public void Evaluate_NotALoginScreenSession_IsIgnored(string service, string user, int uid)
    {
        var policy = new DualBootPolicy(EnabledForDeni);

        Assert.Equal(DualBootAction.None, policy.Evaluate(Login(user, uid, service)));
    }

    [Fact]
    public void Evaluate_SystemAccount_IsIgnoredEvenIfConfigured()
    {
        var policy = new DualBootPolicy(new DualBootOptions { Enabled = true, SwitchToWindowsUsers = ["root"] });

        Assert.Equal(DualBootAction.None, policy.Evaluate(Login("root", 0)));
    }

    [Fact]
    public void Evaluate_MissingUserId_IsIgnored()
    {
        var policy = new DualBootPolicy(EnabledForDeni);

        Assert.Equal(DualBootAction.None, policy.Evaluate(Login("deni", null)));
    }

    [Fact]
    public void Evaluate_UserNameIsCaseSensitive()
    {
        var policy = new DualBootPolicy(EnabledForDeni);

        Assert.Equal(DualBootAction.StayInLinux, policy.Evaluate(Login("Deni", 1000)));
    }

    [Fact]
    public void Evaluate_ConfiguredNamesAreTrimmed()
    {
        var policy = new DualBootPolicy(new DualBootOptions { Enabled = true, SwitchToWindowsUsers = [" deni ", ""] });

        Assert.Equal(DualBootAction.SwitchToWindows, policy.Evaluate(Login("deni", 1000)));
    }

    private static SuccessfulLoginEvent Login(string user, int? uid, string service = "lightdm") => new()
    {
        Timestamp = DateTimeOffset.Now,
        UserName = user,
        UserId = uid,
        Service = service,
        ComputerName = "test-pc"
    };
}