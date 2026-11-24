using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using SecurityMonitor.Api.Security;

namespace SecurityMonitor.Tests.Api;

public sealed class ApiSecurityTests : IDisposable
{
    private const string Token = "test-token";

    private readonly string _tokenFile = Path.GetTempFileName();
    private readonly WebApplicationFactory<ApiToken> _factory;

    public ApiSecurityTests()
    {
        File.WriteAllText(_tokenFile, Token);

        _factory = new WebApplicationFactory<ApiToken>().WithWebHostBuilder(builder => builder.UseSetting("Api:TokenFile", _tokenFile));
    }

    [Fact]
    public async Task Status_WithoutToken_IsRejected()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/api/status");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Status_WithWrongToken_IsRejected() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await SendStatusRequest("wrong-token")).StatusCode);

    [Fact]
    public async Task Status_WithToken_ReturnsTheService()
    {
        HttpResponseMessage response = await SendStatusRequest(Token);

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"service\":\"SecurityMonitor.Api\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Status_FromForeignHost_IsRejectedEvenWithToken() =>
        Assert.Equal(HttpStatusCode.BadRequest, (await SendStatusRequest(Token, host: "evil.example")).StatusCode);

    [Fact]
    public async Task MissingTokenFile_RejectsEveryRequest()
    {
        File.Delete(_tokenFile);

        Assert.Equal(HttpStatusCode.Unauthorized, (await SendStatusRequest(Token)).StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
        File.Delete(_tokenFile);
    }

    private async Task<HttpResponseMessage> SendStatusRequest(string token, string host = "localhost")
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Host = host;

        return await _factory.CreateClient().SendAsync(request);
    }
}