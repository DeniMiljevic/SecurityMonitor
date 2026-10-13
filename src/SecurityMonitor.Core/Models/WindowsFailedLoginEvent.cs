namespace SecurityMonitor.Core.Models;

public sealed record WindowsFailedLoginEvent : FailedLoginEvent
{
    public required int EventId { get; init; }
    public required int LogonType { get; init; }
    public string? Domain { get; init; }
    public required uint FailureStatus { get; init; }
    public required uint FailureSubStatus { get; init; }
    public string? WorkstationName { get; init; }
    public string? CallerProcessName { get; init; }

}
