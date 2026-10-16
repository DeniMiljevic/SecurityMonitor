namespace SecurityMonitor.Infrastructure.Linux.Webcam;

public sealed class LinuxWebcamOptions
{
    public const string SectionName = "LinuxWebcam";

    public string Device { get; init; } = "/dev/video0";
    public double WarmupSeconds { get; init; } = 1;
    public int TimeoutSeconds { get; init; } = 10;
}
