namespace SecurityMonitor.Core.Configuration;

public sealed class SecurityMonitorOptions
{
    public const string SectionName = "SecurityMonitor";
    public int FailedAttemptsBeforeCapture { get; init; } = 1;
    public int AttemptWindowMinutes { get; init; } = 5;
    public int CaptureCooldownMinutes { get; init; } = 0;
    public string EvidenceDirectory { get; init; } = @"%ProgramData%\SecurityMonitor\Evidence";
}
