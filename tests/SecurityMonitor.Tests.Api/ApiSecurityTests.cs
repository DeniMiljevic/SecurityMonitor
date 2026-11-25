using System.Net;
using SecurityMonitor.Tests.Api.Fakes;

namespace SecurityMonitor.Tests.Api;

public sealed class ApiSecurityTests : IDisposable
{
    private readonly FakeApi _api = new();

    [Fact]
    public async Task Status_WithoutToken_IsRejected() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.GetStatusAsync()).StatusCode);

    [Fact]
    public async Task Status_WithWrongToken_IsRejected() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.GetStatusAsync("wrong-token")).StatusCode);

    [Fact]
    public async Task Status_WithToken_ReturnsTheService()
    {
        HttpResponseMessage response = await _api.GetStatusAsync(FakeApi.Token);

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"service\":\"SecurityMonitor.Api\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Status_FromForeignHost_IsRejectedEvenWithToken() =>
        Assert.Equal(HttpStatusCode.BadRequest, (await _api.GetStatusAsync(FakeApi.Token, host: "evil.example")).StatusCode);

    [Fact]
    public async Task MissingTokenFile_RejectsEveryRequest()
    {
        File.Delete(_api.TokenFile);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.GetStatusAsync(FakeApi.Token)).StatusCode);
    }

    public void Dispose() => _api.Dispose();
}