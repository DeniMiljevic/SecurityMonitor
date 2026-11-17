using Microsoft.EntityFrameworkCore;

namespace SecurityMonitor.Infrastructure.Data;

public sealed class SecurityMonitorDbContext : DbContext
{
    public SecurityMonitorDbContext(DbContextOptions<SecurityMonitorDbContext> options) : base(options) { }

    public DbSet<IncidentEntity> Incidents => Set<IncidentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<IncidentEntity>(incident =>
        {
            incident.ToTable("Incidents");
            incident.HasIndex(i => i.TimestampUtc);
        });
}
