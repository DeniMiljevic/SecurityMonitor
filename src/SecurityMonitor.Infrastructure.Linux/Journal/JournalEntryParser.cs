using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Linux.Journal;

public static partial class JournalEntryParser
{
    [GeneratedRegex(@"^pam_unix\((?<service>[^:()]+):auth\): authentication failure;(?<fields>.*)$")]
    private static partial Regex PamAuthFailure();

    public static LinuxFailedLoginEvent? TryParse(string jsonLine)
    {
        if (string.IsNullOrWhiteSpace(jsonLine))
            return null;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(jsonLine);
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            JsonElement entry = document.RootElement;
            if (entry.ValueKind != JsonValueKind.Object)
                return null;

            string? message = GetString(entry, "MESSAGE");
            if (message is null)
                return null;

            Match match = PamAuthFailure().Match(message);
            if (!match.Success)
                return null;

            Dictionary<string, string> fields = ParseFields(match.Groups["fields"].Value);

            return new LinuxFailedLoginEvent
            {
                Timestamp = ParseTimestamp(GetString(entry, "__REALTIME_TIMESTAMP")),
                Service = match.Groups["service"].Value,
                Terminal = GetField(fields, "tty"),
                UserName = GetField(fields, "user"),
                SourceNetworkAddress = GetField(fields, "rhost"),
                ComputerName = GetString(entry, "_HOSTNAME") ?? Environment.MachineName
            };
        }
    }

    private static string? GetString(JsonElement entry, string name) =>
        entry.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static Dictionary<string, string> ParseFields(string text)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (string part in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            int equals = part.IndexOf('=');
            if (equals > 0)
                fields[part[..equals]] = part[(equals + 1)..];
        }

        return fields;
    }

    private static string? GetField(Dictionary<string, string> fields, string name) =>
        fields.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static DateTimeOffset ParseTimestamp(string? microseconds) =>
        long.TryParse(microseconds, NumberStyles.None, CultureInfo.InvariantCulture, out long value) ? DateTimeOffset.UnixEpoch.AddTicks(value * 10).ToLocalTime() : DateTimeOffset.Now;
}
