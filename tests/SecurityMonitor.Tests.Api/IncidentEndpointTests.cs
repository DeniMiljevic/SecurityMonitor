using System.Net;
using SecurityMonitor.Contracts;
using SecurityMonitor.Contracts.Monitor;
using SecurityMonitor.Tests.Api.Fakes;

namespace SecurityMonitor.Tests.Api;

public sealed class IncidentEndpointTests : IDisposable
{
    private readonly FakeApi _api = new();

    [Fact]
    public async Task Incidents_WithoutToken_IsRejected() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.GetAsync("/api/incidents", token: null)).StatusCode);

    [Fact]
    public async Task Incidents_ReturnsTheIncidents()
    {
        _api.Monitor.Incidents.Add(new IncidentSummary(1, DateTimeOffset.UnixEpoch, "Pin", "user", "PC", 3, false, "No camera found"));

        HttpResponseMessage response = await _api.GetAsync("/api/incidents");

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"authenticationMethod\":\"Pin\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Incidents_WithoutTake_AsksForFifty()
    {
        await _api.GetAsync("/api/incidents");

        Assert.Equal(50, _api.Monitor.RequestedTake);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(IMonitorService.MaxIncidents + 1)]
    public async Task Incidents_WithTakeOutOfRange_IsRejected(int take)
    {
        HttpResponseMessage response = await _api.GetAsync($"/api/incidents?take={take}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(_api.Monitor.RequestedTake);
    }

    [Fact]
    public async Task Incidents_WhenMonitorIsUnreachable_ReturnsServiceUnavailable()
    {
        _api.Monitor.Status = null;

        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await _api.GetAsync("/api/incidents")).StatusCode);
    }

    public void Dispose() => _api.Dispose();
}