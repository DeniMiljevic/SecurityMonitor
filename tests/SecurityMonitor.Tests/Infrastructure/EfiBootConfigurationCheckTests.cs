using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Infrastructure.Linux.Boot;
using SecurityMonitor.Infrastructure.Linux.Processes;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Infrastructure;

public class EfiBootConfigurationCheckTests
{
    [Fact]
    public async Task CorrectSetup_HasNoProblems()
    {
        IReadOnlyList<string> problems = await Check(EfiBootEntryParserTests.DualBootOutput).FindProblemsAsync();

        Assert.Empty(problems);
    }

    [Fact]
    public async Task WindowsEntryMissing_IsReported()
    {
        string output = EfiBootEntryParserTests.DualBootOutput.Replace("Boot0004* Windows Boot Manager", "Boot0004* Something Else");

        IReadOnlyList<string> problems = await Check(output).FindProblemsAsync();

        Assert.Contains("Windows Boot Manager", Assert.Single(problems));
    }

    [Fact]
    public async Task WindowsFirstInBootOrder_IsReported()
    {
        string output = EfiBootEntryParserTests.DualBootOutput.Replace("BootOrder: 0003,0004,", "BootOrder: 0004,0003,");

        IReadOnlyList<string> problems = await Check(output).FindProblemsAsync();

        string problem = Assert.Single(problems);
        Assert.Contains("not first", problem);
        Assert.Contains("'Windows Boot Manager'", problem);
    }

    [Fact]
    public async Task EfibootmgrFails_IsReported()
    {
        var runner = new FakeCommandRunner(_ => new CommandResult(2, "", "EFI variables are not supported on this system."));

        IReadOnlyList<string> problems = await new EfiBootConfigurationCheck(new DualBootOptions(), runner).FindProblemsAsync();

        Assert.Contains("not supported", Assert.Single(problems));
    }

    private static EfiBootConfigurationCheck Check(string efibootmgrOutput) =>
        new(new DualBootOptions(), new FakeCommandRunner(_ => new CommandResult(0, efibootmgrOutput, "")));
}