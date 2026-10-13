using System.Diagnostics.Eventing.Reader;

using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Windows.WindowsEventLog;

public sealed class WindowsFailedLoginWatcher : IFailedLoginWatcher
{
    private const string SecurityLogName = "Security";
    private const string FailedLogonQuery = "*[System[(EventID=4625)]]";

    private static readonly XNamespace EventNamespace = "http://schemas.microsoft.com/win/2004/08/events/event";

    private readonly ILogger<WindowsFailedLoginWatcher> _logger;
    private EventLogWatcher? _watcher;

    public WindowsFailedLoginWatcher(ILogger<WindowsFailedLoginWatcher> logger) =>
        _logger = logger;

    public event EventHandler<FailedLoginEvent>? FailedLoginDetected;

    public void Start()
    {
        if (_watcher is not null)
            return;

        var query = new EventLogQuery(SecurityLogName, PathType.LogName, FailedLogonQuery);
        var watcher = new EventLogWatcher(query);
        watcher.EventRecordWritten += OnEventRecordWritten;

        try
        {
            watcher.Enabled = true;
        }
        catch (UnauthorizedAccessException)
        {
            watcher.Dispose();
            _logger.LogError("Access to the Security log was denied. Run the app as administrator.");
            throw;
        }

        _watcher = watcher;
        _logger.LogInformation("Watching the Security log for failed logons (Event ID 4625)");
    }

    public void Stop()
    {
        if (_watcher is null)
            return;

        _watcher.Enabled = false;
        _watcher.EventRecordWritten -= OnEventRecordWritten;
        _watcher.Dispose();
        _watcher = null;

        _logger.LogInformation("Stopped watching the Security log");
    }

    public void Dispose() => Stop();

    private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
    {
        try
        {
            if (e.EventException is not null)
            {
                _logger.LogError(e.EventException, "The event log reported an error");
                return;
            }

            using EventRecord? record = e.EventRecord;
            if (record is null)
                return;

            FailedLoginEvent loginEvent = Parse(record);
            FailedLoginDetected?.Invoke(this, loginEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a 4625 event");
        }
    }

    private static FailedLoginEvent Parse(EventRecord record)
    {
        var data = new Dictionary<string, string>();
        XDocument xml = XDocument.Parse(record.ToXml());

        foreach (XElement element in xml.Descendants(EventNamespace + "Data"))
        {
            string? name = element.Attribute("Name")?.Value;
            if (name is not null)
                data[name] = element.Value;
        }

        int.TryParse(Get(data, "LogonType"), out int logonType);

        return new FailedLoginEvent
        {
            Timestamp = record.TimeCreated is DateTime created ? new DateTimeOffset(created) : DateTimeOffset.Now,
            EventId = record.Id,
            LogonType = logonType,
            UserName = Get(data, "TargetUserName"),
            Domain = Get(data, "TargetDomainName"),
            FailureStatus = ParseHex(Get(data, "Status")),
            FailureSubStatus = ParseHex(Get(data, "SubStatus")),
            SourceNetworkAddress = Get(data, "IpAddress"),
            WorkstationName = Get(data, "WorkstationName"),
            CallerProcessName = Get(data, "ProcessName"),
            ComputerName = record.MachineName
        };
    }

    private static string? Get(Dictionary<string, string> data, string name)
    {
        if (!data.TryGetValue(name, out string? value))
            return null;

        if (string.IsNullOrWhiteSpace(value) || value == "-")
            return null;

        return value;
    }

    private static uint ParseHex(string? value)
    {
        if (value is null)
            return 0;

        string digits = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;

        return uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result : 0;
    }
}
