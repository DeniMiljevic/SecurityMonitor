using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class FailedLoginMonitor
{
    private readonly IFailedLoginClassifier _classifier;
    private readonly FailedLoginPolicy _policy;
    private readonly IWebcamCapture _webcam;
    private readonly IEvidenceStorage _storage;
    private readonly INotifier _notifier;
    private readonly NotificationThrottle _throttle;
    private readonly ILogger<FailedLoginMonitor> _logger;

    public FailedLoginMonitor(
        IFailedLoginClassifier classifier,
        FailedLoginPolicy policy,
        IWebcamCapture webcam,
        IEvidenceStorage storage,
        INotifier notifier,
        NotificationThrottle throttle,
        ILogger<FailedLoginMonitor> logger)
    {
        _classifier = classifier;
        _policy = policy;
        _webcam = webcam;
        _storage = storage;
        _notifier = notifier;
        _throttle = throttle;
        _logger = logger;
    }

    public async Task<SecurityIncident?> HandleAsync(FailedLoginEvent loginEvent, CancellationToken ct = default)
    {
        if (!_classifier.IsPhysicalAttempt(loginEvent))
        {
            _logger.LogDebug("Ignored failed logon: {LoginEvent}", loginEvent);
            return null;
        }

        AuthenticationMethod method = _classifier.GetAuthenticationMethod(loginEvent);
        _logger.LogInformation("Relevant failed login detected: {Method}, user {UserName}", method, loginEvent.UserName ?? "(Unknown)");

        PolicyDecision decision = _policy.Evaluate(loginEvent);

        if (!decision.ShouldCapture)
        {
            if (decision.InCooldown)
                _logger.LogInformation("Failed login count: {Attempts}/{Threshold} (capture cooldown active)", decision.AttemptsInWindow, decision.Threshold);
            else
                _logger.LogInformation("Failed login count: {Attempts}/{Threshold}", decision.AttemptsInWindow, decision.Threshold);

            return null;
        }

        _logger.LogWarning("Failed login threshold reached ({Attempts}/{Threshold})", decision.AttemptsInWindow, decision.Threshold);

        _logger.LogInformation("Attempting webcam capture");
        CaptureResult capture = await CaptureSafelyAsync(ct);

        if (capture.Success)
        {
            _logger.LogInformation("Webcam capture succeeded");
        }
        else
        {
            _logger.LogWarning("Webcam capture failed: {Reason}", capture.FailureReason);
        }

        var incident = new SecurityIncident(loginEvent, method, decision.AttemptsInWindow, capture);
        await SaveSafelyAsync(incident, ct);

        if (_throttle.ShouldNotify(loginEvent.Timestamp))
            await NotifySafelyAsync(incident, ct);
        else
            _logger.LogInformation("Notification skipped (cooldown active)");

        return incident;
    }

    private async Task<CaptureResult> CaptureSafelyAsync(CancellationToken ct)
    {
        try
        {
            return await _webcam.CaptureAsync(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webcam capture threw an unexpected exception");
            return CaptureResult.Failed($"Unexpected camera error: {ex.Message}");
        }
    }

    private async Task SaveSafelyAsync(SecurityIncident incident, CancellationToken ct)
    {
        try
        {
            string folder = await _storage.SaveAsync(incident, ct);
            _logger.LogInformation("Evidence saved to {Folder}", folder);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save evidence");
        }
    }

    private async Task NotifySafelyAsync(SecurityIncident incident, CancellationToken ct)
    {
        try
        {
            await _notifier.NotifyAsync(incident, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
        }
    }
}
