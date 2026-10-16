using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Processes;

namespace SecurityMonitor.Infrastructure.Linux.Webcam;

public sealed class LinuxWebcamCapture : IWebcamCapture
{
    private const string Ffmpeg = "ffmpeg";

    private readonly LinuxWebcamOptions _options;
    private readonly ICommandRunner _runner;
    private readonly ILogger<LinuxWebcamCapture> _logger;

    public LinuxWebcamCapture(LinuxWebcamOptions options, ICommandRunner runner, ILogger<LinuxWebcamCapture> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Device))
            throw new ArgumentException("LinuxWebcam.Device must be set.", nameof(options));

        if (options.WarmupSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "LinuxWebcam.WarmupSeconds cannot be negative.");

        if (options.TimeoutSeconds < 1)
            throw new ArgumentOutOfRangeException(nameof(options), "LinuxWebcam.TimeoutSeconds must be at least 1.");

        _options = options;
        _runner = runner;
        _logger = logger;
    }

    public async Task<CaptureResult> CaptureAsync(CancellationToken ct = default)
    {
        DirectoryInfo folder = Directory.CreateTempSubdirectory("securitymonitor-");
        string photo = Path.Combine(folder.FullName, "photo.jpg");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        try
        {
            CommandResult result = await _runner.RunAsync(Ffmpeg, BuildArguments(photo), timeout.Token);
            if (result.ExitCode != 0)
                return CaptureResult.Failed(DescribeError(result));

            if (!File.Exists(photo))
                return CaptureResult.Failed("ffmpeg finished but did not save a photo");

            byte[] jpeg = await File.ReadAllBytesAsync(photo, ct);
            return jpeg.Length > 0 ? CaptureResult.Succeeded(jpeg) : CaptureResult.Failed("ffmpeg saved an empty photo");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return CaptureResult.Failed($"The camera did not respond within {_options.TimeoutSeconds} s");
        }
        catch (Win32Exception)
        {
            return CaptureResult.Failed("ffmpeg is not installed (sudo apt install ffmpeg)");
        }
        finally
        {
            DeleteQuietly(folder);
        }
    }

    private List<string> BuildArguments(string photo) =>
    [
        "-hide_banner",
        "-loglevel", "error",
        "-f", "video4linux2",
        "-i", _options.Device,
        "-ss", _options.WarmupSeconds.ToString("0.###", CultureInfo.InvariantCulture),
        "-frames:v", "1",
        "-q:v", "2",
        "-y", photo
    ];

    private string DescribeError(CommandResult result)
    {
        string error = result.Error.Trim();

        if (error.Contains("No such file or directory", StringComparison.OrdinalIgnoreCase))
            return $"No camera found ({_options.Device} does not exist)";

        if (error.Contains("Device or resource busy", StringComparison.OrdinalIgnoreCase))
            return "The camera is busy (used by another program)";

        if (error.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
            return "Access to the camera was denied";

        if (error.Contains("Inappropriate ioctl", StringComparison.OrdinalIgnoreCase))
            return $"{_options.Device} is not a camera image device. Try another /dev/videoN.";

        string firstLine = error.Split('\n', 2)[0].Trim();
        return firstLine.Length > 0 ? $"Camera error: {firstLine}" : $"ffmpeg failed with exit code {result.ExitCode}";
    }

    private void DeleteQuietly(DirectoryInfo folder)
    {
        try
        {
            folder.Delete(recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not delete the temporary photo folder {Folder}: {Error}", folder.FullName, ex.Message);
        }
    }
}
