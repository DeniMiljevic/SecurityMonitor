using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Windows.Webcam;

public sealed class ServiceSessionWebcamCapture : IWebcamCapture
{
    public Task<CaptureResult> CaptureAsync(CancellationToken ct = default) =>
        Task.FromResult(CaptureResult.Failed("Camera is not available to a Windows Service (Session 0)"));
}
