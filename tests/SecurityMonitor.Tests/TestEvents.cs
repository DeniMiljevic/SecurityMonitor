using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests;

internal static class TestEvents
{
    public static readonly DateTimeOffset BaseTime = new(2026, 10, 10, 22, 0, 0, TimeSpan.FromHours(2));

    public static FailedLoginEvent PinFailure(DateTimeOffset? time = null) => new()
    {
        Timestamp = time ?? BaseTime,
        EventId = 4625,
        LogonType = 2,
        UserName = null,
        Domain = null,
        FailureStatus = 0xC000006D,
        FailureSubStatus = 0xC0000380,
        SourceNetworkAddress = "127.0.0.1",
        WorkstationName = null,
        CallerProcessName = @"C:\Windows\System32\svchost.exe",
        ComputerName = "TEST-PC"
    };

    public static FailedLoginEvent AtMinute(double minutes) =>
        PinFailure(BaseTime.AddMinutes(minutes));
}
