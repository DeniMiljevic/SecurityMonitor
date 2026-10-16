namespace SecurityMonitor.Core.Interfaces;

public interface IBootConfigurationCheck
{
    Task<IReadOnlyList<string>> FindProblemsAsync(CancellationToken ct = default);
}
