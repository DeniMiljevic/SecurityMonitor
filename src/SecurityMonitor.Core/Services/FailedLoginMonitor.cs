using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class FailedLoginMonitor
{
    private readonly FailedLoginClassifier _classifier;
    private readonly FailedLoginPolicy _policy;
    private readonly IWebcamCapture _webcam;
    private readonly IEvidenceStorage _storage;
    private readonly ILogger<FailedLoginMonitor> _logger;

    public FailedLoginMonitor(
        FailedLoginClassifier classifier,
        FailedLoginPolicy policy,
        IWebcamCapture webcam,
        IEvidenceStorage storage,
        ILogger<FailedLoginMonitor> logger)
    {
        _classifier = classifier;
        _policy = policy;
        _webcam = webcam;
        _storage = storage;
        _logger = logger;
    }

    public async Task<SecurityIncident?> HandleAsync(FailedLoginEvent loginEvent, CancellationToken ct = default)
    {
        if (!_classifier.IsPhysicalAttempt(loginEvent))
        {
            _logger.LogDebug("Ignored failed logon: LogonType {LogonType}, source {Source}", loginEvent.LogonType, loginEvent.SourceNetworkAddress ?? "-");
            return null;
        }

        AuthenticationMethod method = _classifier.GetAuthenticationMethod(loginEvent);
        _logger.LogInformation("Relevant failed login detected: {Method}, LogonType {LogonType}, user {UserName}", method, loginEvent.LogonType, loginEvent.UserName ?? "(unknown)");

        PolicyDecision decision = _policy.Evaluate(loginEvent);

        if (!decision.ShouldCapture)
        {
            if (decision.InCooldown)
            {
                _logger.LogInformation("Failed login count: {Attempts}/{Threshold} (capture cooldown active)", decision.AttemptsInWindow, decision.Threshold);
            }
            else
            {
                _logger.LogInformation("Failed login count: {Attempts}/{Threshold}", decision.AttemptsInWindow, decision.Threshold);
            }

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
}
