namespace SecurityMonitor.Core.Models;

public sealed record BootSwitchResult
{
    public bool Success { get; private init; }
    public bool DryRun { get; private init; }
    public string? FailureReason { get; private init; }

    private BootSwitchResult() { }

    public static BootSwitchResult Rebooting() =>
        new() { Success = true };

    public static BootSwitchResult Simulated() =>
        new() { Success = true, DryRun = true };

    public static BootSwitchResult Failed(string reason) =>
        new() { Success = false, FailureReason = reason };
}
