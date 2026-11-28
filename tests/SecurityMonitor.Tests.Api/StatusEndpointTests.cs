using SecurityMonitor.Contracts.Monitor;
using SecurityMonitor.Tests.Api.Fakes;

namespace SecurityMonitor.Tests.Api;

public sealed class StatusEndpointTests : IDisposable
{
    private readonly FakeApi _api = new();

    [Fact]
    public async Task Status_IncludesTheMonitorStatus()
    {
        _api.Monitor.Status = new MonitorStatus("2.0.0.0", DateTimeOffset.UnixEpoch, IncidentCount: 7);

        HttpResponseMessage response = await _api.GetStatusAsync(FakeApi.Token);

        Assert.Contains("\"incidentCount\":7", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Status_WhenMonitorIsUnreachable_StillAnswersWithoutIt()
    {
        _api.Monitor.Status = null;

        HttpResponseMessage response = await _api.GetStatusAsync(FakeApi.Token);

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"monitor\":null", await response.Content.ReadAsStringAsync());
    }

    public void Dispose() => _api.Dispose();
}