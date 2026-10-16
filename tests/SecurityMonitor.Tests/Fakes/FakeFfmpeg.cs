using SecurityMonitor.Infrastructure.Linux.Processes;

namespace SecurityMonitor.Tests.Fakes;

public sealed class FakeFfmpeg : ICommandRunner
{
    private readonly Func<string, CancellationToken, Task<CommandResult>> _run;

    private FakeFfmpeg(Func<string, CancellationToken, Task<CommandResult>> run) =>
        _run = run;

    public IReadOnlyList<string> LastArguments { get; private set; } = [];

    public static FakeFfmpeg SavesPhoto(byte[] jpeg) => new(async (photo, ct) =>
    {
        await File.WriteAllBytesAsync(photo, jpeg, ct);
        return new CommandResult(0, "", "");
    });

    public static FakeFfmpeg SavesNothing() =>
        new((_, _) => Task.FromResult(new CommandResult(0, "", "")));

    public static FakeFfmpeg FailsWith(string error) =>
        new((_, _) => Task.FromResult(new CommandResult(1, "", error)));

    public static FakeFfmpeg NotInstalled() =>
        new((_, _) => throw new System.ComponentModel.Win32Exception("No such file or directory"));

    public static FakeFfmpeg Hangs() => new(async (_, ct) =>
    {
        await Task.Delay(Timeout.Infinite, ct);
        return new CommandResult(0, "", "");
    });

    public Task<CommandResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        LastArguments = arguments;
        return _run(arguments[^1], ct);
    }
}
