using SecurityMonitor.Core.Interfaces;

namespace SecurityMonitor.Tests.Fakes;

public sealed class FakeBootConfigurationCheck : IBootConfigurationCheck
{
    private readonly Func<IReadOnlyList<string>> _problems;

    public FakeBootConfigurationCheck(params string[] problems) =>
        _problems = () => problems;

    public FakeBootConfigurationCheck(Exception error) =>
        _problems = () => throw error;

    public Task<IReadOnlyList<string>> FindProblemsAsync(CancellationToken ct = default) =>
        Task.FromResult(_problems());
}
