namespace SecurityMonitor.Core.Configuration;

public sealed class SecurityMonitorOptions
{
    public const string SectionName = "SecurityMonitor";

    public int FailedAttemptsBeforeCapture { get; set; } = 1;
    public int AttemptWindowMinutes { get; set; } = 5;
    public int CaptureCooldownMinutes { get; set; } = 0;
}
