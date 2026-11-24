using System.Security.Cryptography;
using System.Text;

namespace SecurityMonitor.Api.Security;

public sealed class ApiToken
{
    private readonly byte[]? _token;

    public ApiToken(IConfiguration config, ILogger<ApiToken> logger)
    {
        string path = Environment.ExpandEnvironmentVariables(config["Api:TokenFile"] ?? "");

        try
        {
            string token = File.ReadAllText(path).Trim();

            if (token.Length > 0)
                _token = Encoding.UTF8.GetBytes(token);
            else
                logger.LogError("The API token file {Path} is empty. Every request will be rejected.", path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogError("Could not read the API token file {Path}: {Error} Every request will be rejected", path, ex.Message);
        }
    }

    public bool Matches(string? presented) =>
        _token is not null
        && presented is not null
        && CryptographicOperations.FixedTimeEquals(_token, Encoding.UTF8.GetBytes(presented));
}
