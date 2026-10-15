using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Services;

public class DualBootMonitorTests
{
    [Fact]
    public async Task WindowsUserLogin_SwitchesToWindows()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Rebooting());

        await Monitor(switcher).HandleAsync(Login("deni", 1000));

        Assert.Equal(1, switcher.Calls);
    }

    [Fact]
    public async Task OtherUserLogin_DoesNotSwitch()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Rebooting());

        await Monitor(switcher).HandleAsync(Login("admin", 1001));

        Assert.Equal(0, switcher.Calls);
    }

    [Fact]
    public async Task GreeterSession_DoesNotSwitch()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Rebooting());

        await Monitor(switcher).HandleAsync(Login("lightdm", 113, "lightdm-greeter"));

        Assert.Equal(0, switcher.Calls);
    }

    [Fact]
    public async Task AfterReboot_SecondLoginDoesNotSwitchAgain()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Rebooting());
        DualBootMonitor monitor = Monitor(switcher);

        await monitor.HandleAsync(Login("deni", 1000));
        await monitor.HandleAsync(Login("deni", 1000));

        Assert.Equal(1, switcher.Calls);
    }

    [Fact]
    public async Task AfterFailure_NextLoginTriesAgain()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Failed("No active UEFI boot entry"));
        DualBootMonitor monitor = Monitor(switcher);

        await monitor.HandleAsync(Login("deni", 1000));
        await monitor.HandleAsync(Login("deni", 1000));

        Assert.Equal(2, switcher.Calls);
    }

    [Fact]
    public async Task AfterDryRun_NextLoginTriesAgain()
    {
        var switcher = new FakeBootSwitcher(BootSwitchResult.Simulated());
        DualBootMonitor monitor = Monitor(switcher);

        await monitor.HandleAsync(Login("deni", 1000));
        await monitor.HandleAsync(Login("deni", 1000));

        Assert.Equal(2, switcher.Calls);
    }

    private static DualBootMonitor Monitor(FakeBootSwitcher switcher) =>
        new(new DualBootPolicy(new DualBootOptions { Enabled = true, SwitchToWindowsUsers = ["deni"] }), switcher, NullLogger<DualBootMonitor>.Instance);

    private static SuccessfulLoginEvent Login(string user, int uid, string service = "lightdm") => new()
    {
        Timestamp = DateTimeOffset.Now,
        UserName = user,
        UserId = uid,
        Service = service,
        ComputerName = "test-pc"
    };
}