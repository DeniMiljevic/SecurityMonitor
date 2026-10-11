using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Core.Services;

namespace SecurityMonitor.Tests;

public class FailedLoginMonitorTests
{
    private static FailedLoginMonitor CreateMonitor(FakeWebcamCapture webcam, int attempts = 1, FakeEvidenceStorage? storage = null) =>
        new(
            new FailedLoginClassifier(),
            new FailedLoginPolicy(new SecurityMonitorOptions
            {
                FailedAttemptsBeforeCapture = attempts,
                AttemptWindowMinutes = 5,
                CaptureCooldownMinutes = 0
            }),
            webcam,
            storage ?? FakeEvidenceStorage.Working(),
            NullLogger<FailedLoginMonitor>.Instance);

    [Fact]
    public async Task HandleAsync_PhysicalAttemptAtThreshold_CapturesPhoto()
    {
        var webcam = FakeWebcamCapture.ThatSucceeds();
        var monitor = CreateMonitor(webcam);

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        Assert.NotNull(incident);
        Assert.True(incident.Capture.Success);
        Assert.Equal(AuthenticationMethod.Pin, incident.Method);
        Assert.Equal(1, webcam.CallCount);
    }

    [Fact]
    public async Task HandleAsync_CameraReturnsFailure_IncidentKeepsFailureReason()
    {
        var webcam = FakeWebcamCapture.ThatFails("No camera found");
        var monitor = CreateMonitor(webcam);

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        Assert.NotNull(incident);
        Assert.False(incident.Capture.Success);
        Assert.Equal("No camera found", incident.Capture.FailureReason);
    }

    [Fact]
    public async Task HandleAsync_CameraThrows_DoesNotCrashMonitor()
    {
        var webcam = FakeWebcamCapture.ThatThrows();
        var monitor = CreateMonitor(webcam);

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        Assert.NotNull(incident);
        Assert.False(incident.Capture.Success);
        Assert.Contains("Simulated camera crash", incident.Capture.FailureReason);
    }

    [Fact]
    public async Task HandleAsync_CameraThrows_MonitorKeepsWorkingForNextAttempt()
    {
        var webcam = FakeWebcamCapture.ThatThrows();
        var monitor = CreateMonitor(webcam);

        await monitor.HandleAsync(TestEvents.AtMinute(0));
        SecurityIncident? second = await monitor.HandleAsync(TestEvents.AtMinute(1));

        Assert.NotNull(second);
        Assert.Equal(2, webcam.CallCount);
    }

    [Fact]
    public async Task HandleAsync_NetworkLogon_IgnoredAndCameraNotUsed()
    {
        var webcam = FakeWebcamCapture.ThatSucceeds();
        var monitor = CreateMonitor(webcam);
        FailedLoginEvent networkEvent = TestEvents.PinFailure() with
        {
            LogonType = 3,
            SourceNetworkAddress = "192.168.1.50"
        };

        SecurityIncident? incident = await monitor.HandleAsync(networkEvent);

        Assert.Null(incident);
        Assert.Equal(0, webcam.CallCount);
    }

    [Fact]
    public async Task HandleAsync_BelowThreshold_CameraNotUsed()
    {
        var webcam = FakeWebcamCapture.ThatSucceeds();
        var monitor = CreateMonitor(webcam, attempts: 3);

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        Assert.Null(incident);
        Assert.Equal(0, webcam.CallCount);
    }

    [Fact]
    public async Task HandleAsync_IncidentCreated_IsSavedToStorage()
    {
        var storage = FakeEvidenceStorage.Working();
        var monitor = CreateMonitor(FakeWebcamCapture.ThatSucceeds(), storage: storage);

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        SecurityIncident saved = Assert.Single(storage.Saved);
        Assert.Same(incident, saved);
    }

    [Fact]
    public async Task HandleAsync_CameraFails_IncidentIsStillSaved()
    {
        var storage = FakeEvidenceStorage.Working();
        var monitor = CreateMonitor(FakeWebcamCapture.ThatFails("No camera found"), storage: storage);

        await monitor.HandleAsync(TestEvents.PinFailure());

        SecurityIncident saved = Assert.Single(storage.Saved);
        Assert.False(saved.Capture.Success);
    }

    [Fact]
    public async Task HandleAsync_StorageThrows_DoesNotCrashMonitor()
    {
        var monitor = CreateMonitor(FakeWebcamCapture.ThatSucceeds(), storage: FakeEvidenceStorage.ThatThrows());

        SecurityIncident? incident = await monitor.HandleAsync(TestEvents.PinFailure());

        Assert.NotNull(incident);
    }

    [Fact]
    public async Task HandleAsync_IgnoredEvent_NothingIsSaved()
    {
        var storage = FakeEvidenceStorage.Working();
        var monitor = CreateMonitor(FakeWebcamCapture.ThatSucceeds(), storage: storage);
        FailedLoginEvent networkEvent = TestEvents.PinFailure() with
        {
            LogonType = 3,
            SourceNetworkAddress = "192.168.1.50"
        };

        await monitor.HandleAsync(networkEvent);

        Assert.Empty(storage.Saved);
    }
}
