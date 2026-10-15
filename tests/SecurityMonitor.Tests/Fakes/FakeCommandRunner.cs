using SecurityMonitor.Infrastructure.Linux.Boot;

namespace SecurityMonitor.Tests.Fakes;

public sealed class FakeCommandRunner : ICommandRunner
{
    private readonly Func<string, CommandResult> _respond;

    public FakeCommandRunner(Func<string, CommandResult> respond) =>
        _respond = respond;

    public List<string> Commands { get; } = [];

    public Task<CommandResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        string command = string.Join(' ', [fileName, .. arguments]);
        Commands.Add(command);
        return Task.FromResult(_respond(command));
    }
}