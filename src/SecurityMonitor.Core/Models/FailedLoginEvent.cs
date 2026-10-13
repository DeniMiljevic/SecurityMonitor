namespace SecurityMonitor.Core.Models;

public abstract record FailedLoginEvent
{
    public required DateTimeOffset Timestamp { get; init; }
    public string? UserName { get; init; }
    public string? SourceNetworkAddress { get; init; }
    public required string ComputerName { get; init; }
}
