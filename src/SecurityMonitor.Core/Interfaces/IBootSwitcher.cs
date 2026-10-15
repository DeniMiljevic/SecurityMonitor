using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface IBootSwitcher
{
    Task<BootSwitchResult> SwitchToWindowsAsync(CancellationToken ct = default);
}
