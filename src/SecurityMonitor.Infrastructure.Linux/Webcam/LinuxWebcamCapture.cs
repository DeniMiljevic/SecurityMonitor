using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Webcam;

public sealed class LinuxWebcamCapture : IWebcamCapture
{
    public Task<CaptureResult> CaptureAsync(CancellationToken ct = default) =>
        Task.FromResult(CaptureResult.Failed("Webcam capture is not implemented on Linux yet"));
}
