using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using SecurityMonitor.Host.Windows.Configuration;
using SecurityMonitor.Infrastructure.Windows.Security;

namespace SecurityMonitor.Host.Windows.Workers;

internal sealed class ApiTokenIssuer : IHostedService
{
    private readonly ApiTokenOptions _options;
    private readonly ILogger<ApiTokenIssuer> _logger;

    public ApiTokenIssuer(IOptions<ApiTokenOptions> options, ILogger<ApiTokenIssuer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        string tokenFile = Environment.ExpandEnvironmentVariables(_options.TokenFile);

        try
        {
            if (!File.Exists(tokenFile))
                CreateToken(tokenFile);

            RestrictedFile.Apply(tokenFile, ResolveReaders());
            _logger.LogInformation("API token is ready at {TokenFile}", tokenFile);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Could not prepare the API token at {TokenFile}, so the API will reject every request", tokenFile);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private void CreateToken(string tokenFile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(tokenFile)!);
        File.WriteAllText(tokenFile, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
        _logger.LogInformation("Created a new API token");
    }

    private List<SecurityIdentifier> ResolveReaders()
    {
        var readers = new List<SecurityIdentifier>();
        IEnumerable<string> accounts = _options.DashboardUsers.Prepend(_options.ServiceAccount).Where(account => !string.IsNullOrWhiteSpace(account));
    
        foreach (string account in accounts)
        {
            try
            {
                readers.Add((SecurityIdentifier)new NTAccount(account).Translate(typeof(SecurityIdentifier)));
            }
            catch (IdentityNotMappedException)
            {
                _logger.LogWarning("Account {Account} does not exist, so it cannot read the API token", account);
            }
        }

        return readers;
    }
}
