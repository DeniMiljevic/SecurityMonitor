using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests.Fakes;

internal sealed class FakeNotifier : INotifier
{
    private readonly bool _throws;
    public List<SecurityIncident> Notified { get; } = [];
    private FakeNotifier(bool throws) => _throws = throws;
    public static FakeNotifier Working() => new(throws: false);
    public static FakeNotifier ThatThrows() => new(throws: true);

    public Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        if (_throws)
            throw new InvalidOperationException("Simulated notifier failure");

        Notified.Add(incident);
        return Task.CompletedTask;
    }
}
