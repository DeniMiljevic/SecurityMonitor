namespace SecurityMonitor.Core.Models;

public sealed record PolicyDecision(bool ShouldCapture, int AttemptsInWindow, int Threshold, bool InCooldown);
