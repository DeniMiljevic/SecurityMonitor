using System.Text.RegularExpressions;

namespace SecurityMonitor.Infrastructure.Linux.Boot;

public sealed record EfiBootEntry(string Number, string Label, bool IsActive);

public sealed record EfiBootState(string? BootCurrent, IReadOnlyList<string> BootOrder, IReadOnlyList<EfiBootEntry> Entries);

public static partial class EfiBootEntryParser
{
    [GeneratedRegex(@"^Boot(?<number>[0-9A-Fa-f]{4})(?<active>\*?)\s+(?<label>[^\t]+)")]
    private static partial Regex BootEntry();

    [GeneratedRegex(@"^BootCurrent:\s*(?<number>[0-9A-Fa-f]{4})")]
    private static partial Regex BootCurrentLine();

    [GeneratedRegex(@"^BootOrder:\s*(?<order>[0-9A-Fa-f,]+)")]
    private static partial Regex BootOrderLine();

    public static IReadOnlyList<EfiBootEntry> ParseEntries(string efibootmgrOutput) =>
        Parse(efibootmgrOutput).Entries;

    public static EfiBootState Parse(string efibootmgrOutput)
    {
        string? bootCurrent = null;
        IReadOnlyList<string> bootOrder = [];
        var entries = new List<EfiBootEntry>();

        foreach (string rawLine in efibootmgrOutput.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');

            Match current = BootCurrentLine().Match(line);
            if (current.Success)
            {
                bootCurrent = current.Groups["number"].Value.ToUpperInvariant();
                continue;
            }

            Match order = BootOrderLine().Match(line);
            if (order.Success)
            {
                bootOrder = order.Groups["order"].Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(number => number.ToUpperInvariant())
                    .ToList();
                continue;
            }

            Match entry = BootEntry().Match(line);
            if (entry.Success)
            {
                entries.Add(new EfiBootEntry(
                    entry.Groups["number"].Value.ToUpperInvariant(),
                    entry.Groups["label"].Value.Trim(),
                    entry.Groups["active"].Value == "*"));
            }
        }

        return new EfiBootState(bootCurrent, bootOrder, entries);
    }
}