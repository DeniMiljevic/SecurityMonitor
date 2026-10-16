using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Webcam;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Infrastructure;

public class LinuxWebcamCaptureTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3];

    [Fact]
    public async Task PhotoSaved_ReturnsJpegAndDeletesTemporaryFile()
    {
        var ffmpeg = FakeFfmpeg.SavesPhoto(Jpeg);

        CaptureResult result = await Capture(ffmpeg).CaptureAsync();

        Assert.True(result.Success);
        Assert.Equal(Jpeg, result.JpegData);
        Assert.False(File.Exists(ffmpeg.LastArguments[^1]));
    }

    [Fact]
    public async Task Arguments_TakeOneFrameFromTheConfiguredDevice()
    {
        var ffmpeg = FakeFfmpeg.SavesPhoto(Jpeg);

        await Capture(ffmpeg, device: "/dev/video2").CaptureAsync();

        string arguments = string.Join(' ', ffmpeg.LastArguments);
        Assert.Contains("-f video4linux2 -i /dev/video2", arguments);
        Assert.Contains("-frames:v 1", arguments);
    }

    [Theory]
    [InlineData("[video4linux2,v4l2 @ 0x5] Cannot open video device /dev/video0: No such file or directory", "No camera found")]
    [InlineData("[video4linux2,v4l2 @ 0x5] Cannot open video device /dev/video0: Device or resource busy", "busy")]
    [InlineData("[video4linux2,v4l2 @ 0x5] Cannot open video device /dev/video0: Permission denied", "denied")]
    [InlineData("[video4linux2,v4l2 @ 0x5] ioctl(VIDIOC_G_INPUT): Inappropriate ioctl for device", "not a camera image device")]
    [InlineData("Something unexpected", "Camera error: Something unexpected")]
    public async Task FfmpegError_IsDescribed(string error, string expected)
    {
        CaptureResult result = await Capture(FakeFfmpeg.FailsWith(error)).CaptureAsync();

        Assert.False(result.Success);
        Assert.Contains(expected, result.FailureReason ?? "");
    }

    [Fact]
    public async Task FfmpegNotInstalled_ReturnsFailure()
    {
        CaptureResult result = await Capture(FakeFfmpeg.NotInstalled()).CaptureAsync();

        Assert.False(result.Success);
        Assert.Contains("not installed", result.FailureReason ?? "");
    }

    [Fact]
    public async Task NoPhotoSaved_ReturnsFailure()
    {
        CaptureResult result = await Capture(FakeFfmpeg.SavesNothing()).CaptureAsync();

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CameraHangs_TimesOut()
    {
        CaptureResult result = await Capture(FakeFfmpeg.Hangs(), timeoutSeconds: 1).CaptureAsync();

        Assert.False(result.Success);
        Assert.Contains("did not respond within 1 s", result.FailureReason ?? "");
    }

    [Fact]
    public void InvalidTimeout_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Capture(FakeFfmpeg.SavesNothing(), timeoutSeconds: 0));
    }

    private static LinuxWebcamCapture Capture(FakeFfmpeg ffmpeg, string device = "/dev/video0", int timeoutSeconds = 10) =>
        new(new LinuxWebcamOptions { Device = device, WarmupSeconds = 1, TimeoutSeconds = timeoutSeconds }, ffmpeg, NullLogger<LinuxWebcamCapture>.Instance);
}