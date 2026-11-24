using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Infrastructure.Common.Storage;

namespace SecurityMonitor.Infrastructure.Data;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddIncidentDatabase(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = Environment.ExpandEnvironmentVariables(config.GetConnectionString("SecurityMonitor")
            ?? throw new InvalidOperationException("Connection string 'SecurityMonitor' is missing."));

        services.AddDbContextFactory<SecurityMonitorDbContext>(options => options.UseSqlite(connectionString));
        services.AddHostedService<DatabaseMigrator>();
        services.AddSingleton<FileEvidenceStorage>();

        services.Replace(ServiceDescriptor.Singleton<IEvidenceStorage>(provider => new DatabaseEvidenceStorage(
            provider.GetRequiredService<FileEvidenceStorage>(),
            provider.GetRequiredService<IDbContextFactory<SecurityMonitorDbContext>>(),
            provider.GetRequiredService<ILogger<DatabaseEvidenceStorage>>())));

        return services;
    }
}
