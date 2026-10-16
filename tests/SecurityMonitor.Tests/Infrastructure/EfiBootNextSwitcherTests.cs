using System.ComponentModel;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Boot;
using SecurityMonitor.Infrastructure.Linux.Processes;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Infrastructure;

public class EfiBootNextSwitcherTests
{
    private static readonly CommandResult Ok = new(0, "", "");

    [Fact]
    public async Task DryRun_OnlyReadsBootEntries()
    {
        var runner = new FakeCommandRunner(Respond());

        BootSwitchResult result = await Switcher(runner, dryRun: true).SwitchToWindowsAsync();

        Assert.True(result.Success);
        Assert.True(result.DryRun);
        Assert.Equal(new[] { "efibootmgr" }, runner.Commands);
    }

    [Fact]
    public async Task Enabled_SetsBootNextAndReboots()
    {
        var runner = new FakeCommandRunner(Respond());

        BootSwitchResult result = await Switcher(runner, dryRun: false).SwitchToWindowsAsync();

        Assert.True(result.Success);
        Assert.False(result.DryRun);
        Assert.Equal(new[] { "efibootmgr", "efibootmgr --bootnext 0004", "systemctl reboot" }, runner.Commands);
    }

    [Fact]
    public async Task WindowsEntryMissing_FailsWithoutChangingAnything()
    {
        string withoutWindows = EfiBootEntryParserTests.DualBootOutput.Replace("Boot0004* Windows Boot Manager", "Boot0004* Something Else");
        var runner = new FakeCommandRunner(Respond(listOutput: withoutWindows));

        BootSwitchResult result = await Switcher(runner, dryRun: false).SwitchToWindowsAsync();

        Assert.False(result.Success);
        Assert.Contains("Windows Boot Manager", result.FailureReason ?? "");
        Assert.Equal(new[] { "efibootmgr" }, runner.Commands);
    }

    [Fact]
    public async Task BootNextFails_DoesNotReboot()
    {
        var runner = new FakeCommandRunner(Respond(bootNext: new CommandResult(1, "", "Permission denied")));

        BootSwitchResult result = await Switcher(runner, dryRun: false).SwitchToWindowsAsync();

        Assert.False(result.Success);
        Assert.DoesNotContain("systemctl reboot", runner.Commands);
    }

    [Fact]
    public async Task RebootFails_ClearsBootNext()
    {
        var runner = new FakeCommandRunner(Respond(reboot: new CommandResult(1, "", "Access denied")));

        BootSwitchResult result = await Switcher(runner, dryRun: false).SwitchToWindowsAsync();

        Assert.False(result.Success);
        Assert.Equal("efibootmgr --delete-bootnext", runner.Commands[^1]);
    }

    [Fact]
    public async Task EfibootmgrNotInstalled_ReturnsFailureInsteadOfCrashing()
    {
        var runner = new FakeCommandRunner(_ => throw new Win32Exception("No such file or directory"));

        BootSwitchResult result = await Switcher(runner, dryRun: false).SwitchToWindowsAsync();

        Assert.False(result.Success);
    }

    [Fact]
    public void NegativeDelay_IsRejected()
    {
        var options = new DualBootOptions { RebootDelaySeconds = -1 };

        Assert.Throws<ArgumentOutOfRangeException>(() => new EfiBootNextSwitcher(options, new FakeCommandRunner(Respond()), NullLogger<EfiBootNextSwitcher>.Instance));
    }

    private static EfiBootNextSwitcher Switcher(FakeCommandRunner runner, bool dryRun) =>
        new(new DualBootOptions { DryRun = dryRun, RebootDelaySeconds = 0 }, runner, NullLogger<EfiBootNextSwitcher>.Instance);

    private static Func<string, CommandResult> Respond(string listOutput = EfiBootEntryParserTests.DualBootOutput, CommandResult? bootNext = null, CommandResult? reboot = null) =>
        command => command switch
        {
            "efibootmgr" => new CommandResult(0, listOutput, ""),
            "efibootmgr --bootnext 0004" => bootNext ?? Ok,
            "systemctl reboot" => reboot ?? Ok,
            _ => Ok
        };
}