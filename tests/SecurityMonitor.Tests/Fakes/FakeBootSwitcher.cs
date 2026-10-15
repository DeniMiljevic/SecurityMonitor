using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests.Fakes;

public sealed class FakeBootSwitcher : IBootSwitcher
{
    private readonly BootSwitchResult _result;

    public FakeBootSwitcher(BootSwitchResult result) =>
        _result = result;

    public int Calls { get; private set; }

    public Task<BootSwitchResult> SwitchToWindowsAsync(CancellationToken ct = default)
    {
        Calls++;
        return Task.FromResult(_result);
    }
}
