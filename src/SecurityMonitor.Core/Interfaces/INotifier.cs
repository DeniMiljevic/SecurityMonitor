using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface INotifier
{
    Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default);
}
