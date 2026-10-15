namespace SecurityMonitor.Infrastructure.Linux.Boot;

public sealed record CommandResult(int ExitCode, string Output, string Error);

public interface ICommandRunner
{
    Task<CommandResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken ct = default);
}
