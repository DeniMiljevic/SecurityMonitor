using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SecurityMonitor.Api.Security;
using SecurityMonitor.Contracts;

namespace SecurityMonitor.Tests.Api.Fakes;

internal sealed class FakeApi : IDisposable
{
    public const string Token = "test-token";

    private readonly WebApplicationFactory<ApiToken> _factory;

    public FakeApi()
    {
        File.WriteAllText(TokenFile, Token);

        _factory = new WebApplicationFactory<ApiToken>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Api:TokenFile", TokenFile);
            builder.ConfigureTestServices(services => services.AddSingleton<IMonitorService>(Monitor));
        });
    }

    public string TokenFile { get; } = Path.GetTempFileName();

    public FakeMonitorService Monitor { get; } = new();

    public Task<HttpResponseMessage> GetStatusAsync() => GetAsync("/api/status", token: null);

    public Task<HttpResponseMessage> GetStatusAsync(string token, string host = "localhost") => GetAsync("/api/status", token, host);

    public async Task<HttpResponseMessage> GetAsync(string path, string? token = Token, string host = "localhost")
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Host = host;

        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _factory.CreateClient().SendAsync(request);
    }

    public void Dispose()
    {
        _factory.Dispose();
        File.Delete(TokenFile);
    }
}