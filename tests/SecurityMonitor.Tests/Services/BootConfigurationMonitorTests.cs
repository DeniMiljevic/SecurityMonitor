using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Services;

public class BootConfigurationMonitorTests
{
    [Fact]
    public async Task NoProblems_SendsNothing()
    {
        var notifier = FakeNotifier.Working();

        await Monitor(new FakeBootConfigurationCheck(), notifier).RunAsync();

        Assert.Equal(0, notifier.WarningAttempts);
    }

    [Fact]
    public async Task Problems_AreSentInOneWarning()
    {
        var notifier = FakeNotifier.Working();
        var check = new FakeBootConfigurationCheck("Windows entry missing", "Linux is not first");

        await Monitor(check, notifier).RunAsync();

        string warning = Assert.Single(notifier.Warnings);
        Assert.Contains("• Windows entry missing", warning);
        Assert.Contains("• Linux is not first", warning);
    }

    [Fact]
    public async Task NetworkNotReady_RetriesUntilSent()
    {
        var notifier = FakeNotifier.FailingWarnings(times: 2);

        await Monitor(new FakeBootConfigurationCheck("Linux is not first"), notifier).RunAsync();

        Assert.Equal(3, notifier.WarningAttempts);
        Assert.Single(notifier.Warnings);
    }

    [Fact]
    public async Task NotifierNeverWorks_GivesUpWithoutCrashing()
    {
        var notifier = FakeNotifier.ThatThrows();

        await Monitor(new FakeBootConfigurationCheck("Linux is not first"), notifier).RunAsync();

        Assert.Equal(10, notifier.WarningAttempts);
        Assert.Empty(notifier.Warnings);
    }

    [Fact]
    public async Task CheckFails_IsReportedAsAProblem()
    {
        var notifier = FakeNotifier.Working();
        var check = new FakeBootConfigurationCheck(new InvalidOperationException("efibootmgr not found"));

        await Monitor(check, notifier).RunAsync();

        Assert.Contains("efibootmgr not found", Assert.Single(notifier.Warnings));
    }

    private static BootConfigurationMonitor Monitor(FakeBootConfigurationCheck check, FakeNotifier notifier) =>
        new(check, notifier, NullLogger<BootConfigurationMonitor>.Instance) { RetryDelay = TimeSpan.Zero };
}