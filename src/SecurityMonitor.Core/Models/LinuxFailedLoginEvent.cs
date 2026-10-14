namespace SecurityMonitor.Core.Models;

public sealed record LinuxFailedLoginEvent : FailedLoginEvent
{
    public required string Service { get; init; }
    public string? Terminal { get; init; }
}
