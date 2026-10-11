using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace SecurityMonitor.Infrastructure.Webcam;

public sealed class WindowsWebcamCapture : IWebcamCapture, IDisposable
{
    private readonly ILogger<WindowsWebcamCapture> _logger;
    private readonly SemaphoreSlim _oneAtATime = new(1, 1);

    public WindowsWebcamCapture(ILogger<WindowsWebcamCapture> logger) =>
        _logger = logger;

    public async Task<CaptureResult> CaptureAsync(CancellationToken ct = default)
    {
        await _oneAtATime.WaitAsync(ct);
        try
        {
            return await CaptureOnceAsync(ct);
        }
        finally
        {
            _oneAtATime.Release();
        }
    }

    private async Task<CaptureResult> CaptureOnceAsync(CancellationToken ct)
    {
        try
        {
            DeviceInformationCollection cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture).AsTask(ct);
            
            if (cameras.Count == 0)
            {
                _logger.LogWarning("No camera found");
                return CaptureResult.Failed("No camera found");
            }

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = cameras[0].Id,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            using var mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(settings).AsTask(ct);

            using var stream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream).AsTask(ct);

            byte[] jpeg = new byte[stream.Size];
            using (var reader = new DataReader(stream.GetInputStreamAt(0)))
            {
                await reader.LoadAsync((uint)stream.Size).AsTask(ct);
                reader.ReadBytes(jpeg);
            }

            _logger.LogInformation("Photo captured ({Size} bytes) from {Camera}", jpeg.Length, cameras[0].Name);
            return CaptureResult.Succeeded(jpeg);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Camera access denied by Windows privacy settings");
            return CaptureResult.Failed("Camera access denied. Check Settings > Privacy & security > Camera > Let desktop apps access your camera.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webcam capture failed");
            return CaptureResult.Failed($"Camera error: {ex.Message}");
        }
    }
    public void Dispose() => _oneAtATime.Dispose();
}
