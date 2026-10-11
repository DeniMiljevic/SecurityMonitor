namespace SecurityMonitor.Core.Models;

public sealed record CaptureResult
{
    public bool Success { get; private init; }
    public byte[]? JpegData { get; private init; }
    public string? FailureReason { get; private init; }

    private CaptureResult() { }

    public static CaptureResult Succeeded(byte[] jpegData) =>
        new() { Success = true, JpegData = jpegData };

    public static CaptureResult Failed(string reason) =>
        new() { Success = false, FailureReason = reason };
}
