using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests.Fakes;

internal sealed class FakeNotifier : INotifier
{
    private readonly bool _throws;
    private int _warningFailuresLeft;

    public List<SecurityIncident> Notified { get; } = [];
    public List<string> Warnings { get; } = [];
    public int WarningAttempts { get; private set; }

    private FakeNotifier(bool throws, int warningFailures = 0)
    {
        _throws = throws;
        _warningFailuresLeft = warningFailures;
    }

    public static FakeNotifier Working() => new(throws: false);
    public static FakeNotifier ThatThrows() => new(throws: true);

    public static FakeNotifier FailingWarnings(int times) => new(throws: false, warningFailures: times);

    public Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        if (_throws)
            throw new InvalidOperationException("Simulated notifier failure");

        Notified.Add(incident);
        return Task.CompletedTask;
    }

    public Task NotifyWarningAsync(string message, CancellationToken ct = default)
    {
        WarningAttempts++;

        if (_throws || _warningFailuresLeft-- > 0)
            throw new HttpRequestException("Simulated network failure");

        Warnings.Add(message);
        return Task.CompletedTask;
    }
}