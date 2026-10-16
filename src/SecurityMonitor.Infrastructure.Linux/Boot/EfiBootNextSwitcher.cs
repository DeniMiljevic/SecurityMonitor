using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Processes;

namespace SecurityMonitor.Infrastructure.Linux.Boot;

public sealed class EfiBootNextSwitcher : IBootSwitcher
{
    private const string Efibootmgr = "efibootmgr";

    private readonly DualBootOptions _options;
    private readonly ICommandRunner _runner;
    private readonly ILogger<EfiBootNextSwitcher> _logger;

    public EfiBootNextSwitcher(DualBootOptions options, ICommandRunner runner, ILogger<EfiBootNextSwitcher> logger)
    {
        if (options.RebootDelaySeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "RebootDelaySeconds cannot be negative.");

        if (string.IsNullOrWhiteSpace(options.WindowsBootEntry))
            throw new ArgumentException("WindowsBootEntry must be set.", nameof(options));

        _options = options;
        _runner = runner;
        _logger = logger;
    }

    public async Task<BootSwitchResult> SwitchToWindowsAsync(CancellationToken ct = default)
    {
        string label = _options.WindowsBootEntry.Trim();

        try
        {
            string? entry = await FindActiveEntryAsync(label, ct);
            if (entry is null)
                return Fail($"No active UEFI boot entry named '{label}' was found");

            if (_options.DryRun)
            {
                _logger.LogWarning("DryRun: would set BootNext to {Entry} ({Label}) and reboot in {Delay} s", entry, label, _options.RebootDelaySeconds);
                return BootSwitchResult.Simulated();
            }

            CommandResult bootNext = await _runner.RunAsync(Efibootmgr, ["--bootnext", entry], ct);
            if (bootNext.ExitCode != 0)
                return Fail($"efibootmgr --bootnext failed: {bootNext.Error.Trim()}");

            return await RebootAsync(entry, label, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Switching to Windows failed");
            return BootSwitchResult.Failed(ex.Message);
        }
    }

    private async Task<string?> FindActiveEntryAsync(string label, CancellationToken ct)
    {
        CommandResult list = await _runner.RunAsync(Efibootmgr, [], ct);
        if (list.ExitCode != 0)
            throw new InvalidOperationException($"efibootmgr failed: {list.Error.Trim()}");

        return EfiBootEntryParser.ParseEntries(list.Output).FirstOrDefault(e => e.IsActive && string.Equals(e.Label, label, StringComparison.OrdinalIgnoreCase))?.Number;
    }

    private async Task<BootSwitchResult> RebootAsync(string entry, string label, CancellationToken ct)
    {
        try
        {
            _logger.LogWarning("BootNext set to {Entry} ({Label}). Rebooting into Windows in {Delay} s", entry, label, _options.RebootDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_options.RebootDelaySeconds), ct);

            CommandResult reboot = await _runner.RunAsync("systemctl", ["reboot"], ct);
            if (reboot.ExitCode == 0)
                return BootSwitchResult.Rebooting();

            await ClearBootNextAsync();
            return Fail($"systemctl reboot failed: {reboot.Error.Trim()}");
        }
        catch (OperationCanceledException)
        {
            await ClearBootNextAsync();
            throw;
        }
    }

    private async Task ClearBootNextAsync()
    {
        try
        {
            CommandResult result = await _runner.RunAsync(Efibootmgr, ["--delete-bootnext"], CancellationToken.None);
            if (result.ExitCode != 0)
                _logger.LogError("Could not clear BootNext: {Error}. The next reboot may start Windows.", result.Error.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not clear BootNext. The next reboot may start Windows.");
        }
    }

    private BootSwitchResult Fail(string reason)
    {
        _logger.LogError("Switching to Windows failed: {Reason}", reason);
        return BootSwitchResult.Failed(reason);
    }
}
