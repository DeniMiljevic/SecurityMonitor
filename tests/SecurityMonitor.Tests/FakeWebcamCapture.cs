using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests;

internal sealed class FakeWebcamCapture : IWebcamCapture
{
    private readonly Func<CaptureResult> _behaviour;

    public int CallCount { get; private set; }

    private FakeWebcamCapture(Func<CaptureResult> behaviour) => _behaviour = behaviour;

    public static FakeWebcamCapture ThatSucceeds() =>
        new(() => CaptureResult.Succeeded([0xFF, 0xD8, 0xFF]));

    public static FakeWebcamCapture ThatFails(string reason) =>
        new(() => CaptureResult.Failed(reason));

    public static FakeWebcamCapture ThatThrows() =>
        new(() => throw new InvalidOperationException("Simulated camera crash"));

    public Task<CaptureResult> CaptureAsync(CancellationToken ct = default)
    {
        CallCount++;
        return Task.FromResult(_behaviour());
    }
}
