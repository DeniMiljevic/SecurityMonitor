namespace SecurityMonitor.Core.Models;

public sealed record SuccessfulLoginEvent
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string UserName { get; init; }
    public required int? UserId { get; init; }
    public required string Service { get; init; }
    public required string ComputerName { get; init; }
}
