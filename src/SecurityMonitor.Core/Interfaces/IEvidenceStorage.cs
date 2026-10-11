using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface IEvidenceStorage
{
    Task<string> SaveAsync(SecurityIncident incident, CancellationToken ct = default);
}
