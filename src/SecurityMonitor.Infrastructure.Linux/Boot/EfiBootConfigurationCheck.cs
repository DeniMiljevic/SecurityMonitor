using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;

namespace SecurityMonitor.Infrastructure.Linux.Boot;

public sealed class EfiBootConfigurationCheck : IBootConfigurationCheck
{
    private readonly DualBootOptions _options;
    private readonly ICommandRunner _runner;

    public EfiBootConfigurationCheck(DualBootOptions options, ICommandRunner runner)
    {
        _options = options;
        _runner = runner;
    }

    public async Task<IReadOnlyList<string>> FindProblemsAsync(CancellationToken ct = default)
    {
        CommandResult list = await _runner.RunAsync("efibootmgr", [], ct);
        if (list.ExitCode != 0)
            return [$"Could not read the UEFI boot entries: {list.Error.Trim()}"];

        EfiBootState state = EfiBootEntryParser.Parse(list.Output);
        string label = _options.WindowsBootEntry.Trim();
        var problems = new List<string>();

        bool windowsEntryExists = state.Entries.Any(e => e.IsActive && string.Equals(e.Label, label, StringComparison.OrdinalIgnoreCase));
        if (!windowsEntryExists)
            problems.Add($"No active UEFI boot entry named '{label}'. A login will not switch to Windows.");

        string? first = state.BootOrder.FirstOrDefault();
        if (state.BootCurrent is not null && first is not null && first != state.BootCurrent)
        {
            string firstLabel = state.Entries.FirstOrDefault(e => e.Number == first)?.Label ?? $"Boot{first}";
            problems.Add($"Linux (Boot{state.BootCurrent}) is not first in the UEFI boot order ({string.Join(",", state.BootOrder)}). A normal start goes to '{firstLabel}' and skips the login gate.");
        }

        return problems;
    }
}
