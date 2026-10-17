using SecurityMonitor.Infrastructure.Common.Firmware;
using SecurityMonitor.Tests.Fakes;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Firmware;

public class UefiBootOrderCheckTests
{
    private const ushort Linux = 0x0003;
    private const ushort Windows = 0x0006;

    [Fact]
    public async Task LinuxFirst_HasNoProblems() =>
       Assert.Empty(await Check(DualBoot().WithBootOrder(Linux, Windows)));

    [Fact]
    public async Task WindowsFirst_IsReported()
    {
        string problem = Assert.Single(await Check(DualBoot().WithBootOrder(Windows, Linux)));

        Assert.Contains("'Windows Boot Manager' (Boot0006) is first", problem);
        Assert.Contains("(0006,0003)", problem);
    }

    [Fact]
    public async Task InactiveLinuxEntry_IsSkippedLikeTheFirmwareDoes()
    {
        FakeUefiVariables variables = new FakeUefiVariables().WithBootOrder(Linux, Windows).WithEntry(Linux, LinuxEntry(active: false)).WithEntry(Windows, WindowsEntry());

        Assert.Contains("(Boot0006) is first", Assert.Single(await Check(variables)));
    }

    [Fact]
    public async Task MissingEntryInBootOrder_IsSkipped() =>
        Assert.Empty(await Check(DualBoot().WithBootOrder(0x0009, Linux, Windows)));

    [Fact]
    public async Task NoBootOrder_IsReported() =>
        Assert.Contains("BootOrder", Assert.Single(await Check(new FakeUefiVariables())));

    [Fact]
    public async Task NoActiveEntry_IsReported()
    {
        FakeUefiVariables variables = new FakeUefiVariables().WithBootOrder(Linux).WithEntry(Linux, LinuxEntry(active: false));

        Assert.Contains("No active", Assert.Single(await Check(variables)));
    }

    private static FakeUefiVariables DualBoot() =>
        new FakeUefiVariables().WithEntry(Linux, LinuxEntry()).WithEntry(Windows, WindowsEntry());

    private static byte[] LinuxEntry(bool active = true) =>
        UefiTestData.LoadOption("ubuntu", @"\EFI\ubuntu\shimx64.efi", active);

    private static byte[] WindowsEntry() =>
        UefiTestData.LoadOption("Windows Boot Manager", @"\EFI\Microsoft\Boot\bootmgfw.efi", optionalData: UefiTestData.WindowsOptionalData);

    private static Task<IReadOnlyList<string>> Check(FakeUefiVariables variables) =>
        new UefiBootOrderCheck(variables).FindProblemsAsync();
}
