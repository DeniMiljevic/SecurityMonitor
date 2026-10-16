using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Common.Notifications;

public sealed class NullNotifier : INotifier
{
    public Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyWarningAsync(string message, CancellationToken ct = default) =>
        Task.CompletedTask;
}
