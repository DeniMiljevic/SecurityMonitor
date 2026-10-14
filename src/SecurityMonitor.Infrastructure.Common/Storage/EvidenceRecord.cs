using System.Text.Json.Serialization;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Common.Storage;

internal sealed record EvidenceRecord
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string AuthenticationMethod { get; init; }
    public string? UserName { get; init; }
    public string? SourceNetworkAddress { get; init; }
    public required string ComputerName { get; init; }
    public required int AttemptsInWindow { get; init; }
    public required bool CaptureSucceeded { get; init; }
    public string? CaptureFailureReason { get; init; }
    public string? PhotoFile { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WindowsDetails? Windows { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LinuxDetails? Linux { get; init; }

    public static EvidenceRecord From(SecurityIncident incident, string? photoFile)
    {
        FailedLoginEvent e = incident.TriggeringEvent;

        return new EvidenceRecord
        {
            Timestamp = e.Timestamp,
            AuthenticationMethod = incident.Method.ToString(),
            UserName = e.UserName,
            SourceNetworkAddress = e.SourceNetworkAddress,
            ComputerName = e.ComputerName,
            AttemptsInWindow = incident.AttemptsInWindow,
            CaptureSucceeded = incident.Capture.Success,
            CaptureFailureReason = incident.Capture.FailureReason,
            PhotoFile = photoFile,
            Windows = e is WindowsFailedLoginEvent windowsEvent ? WindowsDetails.From(windowsEvent) : null,
            Linux = e is LinuxFailedLoginEvent linuxEvent ? LinuxDetails.From(linuxEvent) : null
        };
    }
}

internal sealed record WindowsDetails
{
    public required int EventId { get; init; }
    public required int LogonType { get; init; }
    public string? Domain { get; init; }
    public required string FailureStatus { get; init; }
    public required string FailureSubStatus { get; init; }
    public string? WorkstationName { get; init; }

    public static WindowsDetails From(WindowsFailedLoginEvent e) => new()
    {
        EventId = e.EventId,
        LogonType = e.LogonType,
        Domain = e.Domain,
        FailureStatus = $"0x{e.FailureStatus:X8}",
        FailureSubStatus = $"0x{e.FailureSubStatus:X8}",
        WorkstationName = e.WorkstationName
    };
}

internal sealed record LinuxDetails
{
    public required string Service { get; init; }
    public string? Terminal { get; init; }

    public static LinuxDetails From(LinuxFailedLoginEvent e) => new()
    {
        Service = e.Service,
        Terminal = e.Terminal
    };
}
