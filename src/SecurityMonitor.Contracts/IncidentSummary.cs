namespace SecurityMonitor.Contracts;

public sealed record IncidentSummary(
    long Id,
    DateTimeOffset TimestampUtc,
    string AuthenticationMethod,
    string? UserName,
    string ComputerName,
    int AttemptsInWindow,
    bool CaptureSucceeded,
    string? CaptureFailureReason);
