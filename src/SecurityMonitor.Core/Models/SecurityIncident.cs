namespace SecurityMonitor.Core.Models;

public sealed record SecurityIncident(FailedLoginEvent TriggeringEvent, AuthenticationMethod Method, int AttemptsInWindow, CaptureResult Capture);
