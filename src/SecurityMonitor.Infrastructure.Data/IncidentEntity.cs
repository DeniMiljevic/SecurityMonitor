using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Data;

public sealed class IncidentEntity
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public required string AuthenticationMethod { get; set; }
    public string? UserName { get; set; }
    public required string ComputerName { get; set; }
    public int AttemptsInWindow { get; set; }
    public bool CaptureSucceeded { get; set; }
    public string? CaptureFailureReason { get; set; }
    public required string EvidenceFolder { get; set; }

    public static IncidentEntity From(SecurityIncident incident, string evidenceFolder) => new()
    {
        TimestampUtc = incident.TriggeringEvent.Timestamp.UtcDateTime,
        AuthenticationMethod = incident.Method.ToString(),
        UserName = incident.TriggeringEvent.UserName,
        ComputerName = incident.TriggeringEvent.ComputerName,
        AttemptsInWindow = incident.AttemptsInWindow,
        CaptureSucceeded = incident.Capture.Success,
        CaptureFailureReason = incident.Capture.FailureReason,
        EvidenceFolder = evidenceFolder
    };
}
