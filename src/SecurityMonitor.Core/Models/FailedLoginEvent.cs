namespace SecurityMonitor.Core.Models;

public sealed record FailedLoginEvent
{
    public required DateTimeOffset Timestamp { get; init; }
    public required int EventId { get; init; }
    public required int LogonType { get; init; }

    public string? UserName { get; init; }
    public string? Domain { get; init; }

    public required uint FailureStatus { get; init; }
    public required uint FailureSubStatus { get; init; }

    public string? SourceNetworkAddress { get; init; }
    public string? WorkstationName { get; init; }
    public string? CallerProcessName { get; init; }
    public required string ComputerName { get; init; }
}
