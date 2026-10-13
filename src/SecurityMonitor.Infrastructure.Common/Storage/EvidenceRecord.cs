using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Common.Storage;

internal sealed record EvidenceRecord
{
    public required DateTimeOffset Timestamp { get; init; }
    public required int EventId { get; init; }
    public required int LogonType { get; init; }
    public required string AuthenticationMethod { get; init; }
    public string? UserName { get; init; }
    public string? Domain { get; init; }
    public required string FailureStatus { get; init; }
    public required string FailureSubStatus { get; init; }
    public string? SourceNetworkAddress { get; init; }
    public string? WorkstationName { get; init; }
    public required string ComputerName { get; init; }
    public required int AttemptsInWindow { get; init; }
    public required bool CaptureSucceeded { get; init; }
    public string? CaptureFailureReason { get; init; }
    public string? PhotoFile { get; init; }

    public static EvidenceRecord From(SecurityIncident incident, string? photoFile)
    {
        FailedLoginEvent e = incident.TriggeringEvent;

        return new EvidenceRecord
        {
            Timestamp = e.Timestamp,
            EventId = e.EventId,
            LogonType = e.LogonType,
            AuthenticationMethod = incident.Method.ToString(),
            UserName = e.UserName,
            Domain = e.Domain,
            FailureStatus = $"0x{e.FailureStatus:X8}",
            FailureSubStatus = $"0x{e.FailureSubStatus:X8}",
            SourceNetworkAddress = e.SourceNetworkAddress,
            WorkstationName = e.WorkstationName,
            ComputerName = e.ComputerName,
            AttemptsInWindow = incident.AttemptsInWindow,
            CaptureSucceeded = incident.Capture.Success,
            CaptureFailureReason = incident.Capture.FailureReason,
            PhotoFile = photoFile
        };
    }
}
