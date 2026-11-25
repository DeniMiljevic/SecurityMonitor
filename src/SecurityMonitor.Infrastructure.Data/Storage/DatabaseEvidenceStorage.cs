using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Data.Entities;

namespace SecurityMonitor.Infrastructure.Data.Storage;

public sealed class DatabaseEvidenceStorage : IEvidenceStorage
{
    private readonly IEvidenceStorage _files;
    private readonly IDbContextFactory<SecurityMonitorDbContext> _dbFactory;
    private readonly ILogger<DatabaseEvidenceStorage> _logger;

    public DatabaseEvidenceStorage(
        IEvidenceStorage files,
        IDbContextFactory<SecurityMonitorDbContext> dbFactory,
        ILogger<DatabaseEvidenceStorage> logger)
    {
        _files = files;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<string> SaveAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        string folder = await _files.SaveAsync(incident, ct);

        try
        {
            await using SecurityMonitorDbContext db = await _dbFactory.CreateDbContextAsync(ct);
            db.Incidents.Add(IncidentEntity.From(incident, folder));
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Evidence was saved to {Folder}, but could not be added to the incident database", folder);
        }

        return folder;
    }
}
