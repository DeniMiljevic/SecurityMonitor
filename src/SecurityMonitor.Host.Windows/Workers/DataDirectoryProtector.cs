using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Infrastructure.Windows.Security;

namespace SecurityMonitor.Host.Windows.Workers;

public sealed class DataDirectoryProtector : IHostedService
{
    private readonly SecurityMonitorOptions _options;
    private readonly ILogger<DataDirectoryProtector> _logger;

    public DataDirectoryProtector(SecurityMonitorOptions options, ILogger<DataDirectoryProtector> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        string evidence = Path.GetFullPath(Environment.ExpandEnvironmentVariables(_options.EvidenceDirectory));

        foreach (string directory in new[] { WindowsPaths.DataDirectory, evidence }.Distinct(StringComparer.OrdinalIgnoreCase))
            Protect(directory);

        ProtectSecrets();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private void Protect(string directory)
    {
        try
        {
            AdminOnlyDirectory.Apply(directory);
            _logger.LogInformation("Access to {Directory} is limited to administrators", directory);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not limit access to {Directory} to administratos: {Error} Run the app as administrator", directory, ex.Message);
        }
    }

    private void ProtectSecrets()
    {
        if (!File.Exists(WindowsPaths.SecretsFile))
            return;

        try
        {
            RestrictedFile.Apply(WindowsPaths.SecretsFile, readers: []);
            _logger.LogInformation("Access to {File} is limited to administrators", WindowsPaths.SecretsFile);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not limit access to {File} to administrators: {Error} Run the app as administrator", WindowsPaths.SecretsFile, ex.Message);
        }
    }
}
