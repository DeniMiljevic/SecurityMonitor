using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Notifications;

public sealed class NullNotifier : INotifier
{
    public Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default) =>
        Task.CompletedTask;
}
