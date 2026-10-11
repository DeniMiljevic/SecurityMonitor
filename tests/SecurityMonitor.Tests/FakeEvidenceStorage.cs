using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Tests;

internal sealed class FakeEvidenceStorage : IEvidenceStorage
{
    private readonly bool _shouldThrow;
    public List<SecurityIncident> Saved { get; } = [];
    private FakeEvidenceStorage(bool shouldThrow) => _shouldThrow = shouldThrow;
    public static FakeEvidenceStorage Working() => new(shouldThrow: false);
    public static FakeEvidenceStorage ThatThrows() => new(shouldThrow: true);

    public Task<string> SaveAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        if (_shouldThrow)
            throw new IOException("Simulated disk full");

        Saved.Add(incident);
        return Task.FromResult(@"C:\fake\evidence");
    }
}
