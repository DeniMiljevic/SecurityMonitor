using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface IWebcamCapture
{
    Task<CaptureResult> CaptureAsync(CancellationToken ct = default);
}
