namespace SecurityMonitor.Core.Configuration;

public sealed class DualBootOptions
{
    public const string SectionName = "DualBoot";
    public bool Enabled { get; init; }
    public bool DryRun { get; init; } = true;
    public string[] SwitchToWindowsUsers { get; init; } = [];
    public string WindowsBootEntry { get; init; } = "Windows Boot Manager";
    public int RebootDelaySeconds { get; init; } = 5;
}
