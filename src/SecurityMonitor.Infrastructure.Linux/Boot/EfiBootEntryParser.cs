using System.Text.RegularExpressions;

namespace SecurityMonitor.Infrastructure.Linux.Boot;

public sealed record EfiBootEntry(string Number, string Label, bool IsActive);

public static partial class EfiBootEntryParser
{
    [GeneratedRegex(@"^Boot(?<number>[0-9A-Fa-f]{4})(?<active>\*?)\s+(?<label>[^\t]+)")]
    private static partial Regex BootEntry();

    public static IReadOnlyList<EfiBootEntry> ParseEntries(string efibootmgrOutput)
    {
        var entries = new List<EfiBootEntry>();

        foreach (string line in efibootmgrOutput.Split('\n'))
        {
            Match match = BootEntry().Match(line.TrimEnd('\r'));
            if (!match.Success)
                continue;

            entries.Add(new EfiBootEntry(
                match.Groups["number"].Value.ToUpperInvariant(),
                match.Groups["label"].Value.Trim(),
                match.Groups["active"].Value == "*"));
        }

        return entries;
    }
}
