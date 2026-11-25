using System.IO.Pipes;
using System.Security.Principal;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Contracts;
using SecurityMonitor.Host.Windows.Workers;
using SecurityMonitor.Infrastructure.Windows.Security;
using SecurityMonitor.Tests.Windows.Fakes;
using StreamJsonRpc;

namespace SecurityMonitor.Tests.Windows;

public sealed class MonitorPipeTests
{
    private static readonly MonitorStatus Status = new("1.2.3.4", DateTimeOffset.UnixEpoch, IncidentCount: 5);
    private static readonly IReadOnlyList<IncidentSummary> Incidents =
    [
        new(1, new DateTimeOffset(2026, 10, 10, 20, 0, 0, TimeSpan.Zero), "Pin", "user", "PC", 3, false, "No camera found")
    ];
    private static readonly string CurrentUser = WindowsIdentity.GetCurrent().Name;

    private readonly string _pipeName = $"SecurityMonitor.Tests.{Guid.NewGuid():N}";

    [Fact]
    public async Task AllowedAccount_GetsTheStatus()
    {
        using MonitorPipeListener listener = await StartListenerAsync(clientAccount: CurrentUser);

        Assert.Equal(Status, await CallOverPipeAsync(monitor => monitor.GetStatusAsync(CancellationToken.None)));

        await listener.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task AllowedAccount_GetsTheIncidents()
    {
        using MonitorPipeListener listener = await StartListenerAsync(clientAccount: CurrentUser);

        Assert.Equal(Incidents, await CallOverPipeAsync(monitor => monitor.GetIncidentsAsync(10, CancellationToken.None)));

        await listener.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Pipe_GrantsAccessOnlyToTheServerAndTheClient()
    {
        SecurityIdentifier server = WindowsIdentity.GetCurrent().User!;
        var client = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);

        using NamedPipeServerStream pipe = RestrictedPipe.Create(_pipeName, [client], firstInstance: true);
        PipeSecurity security = pipe.GetAccessControl();

        string[] identities = security
            .GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
            .Cast<PipeAccessRule>()
            .Select(rule => rule.IdentityReference.Value)
            .Order()
            .ToArray();

        Assert.True(security.AreAccessRulesProtected);
        Assert.Equal(new[] { server.Value, client.Value }.Order(), identities);
    }

    [Fact]
    public async Task TakenPipeName_IsNotShared()
    {
        using var squatter = new NamedPipeServerStream(_pipeName, PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        using MonitorPipeListener listener = await StartListenerAsync(clientAccount: CurrentUser);

        await listener.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private async Task<MonitorPipeListener> StartListenerAsync(string clientAccount)
    {
        var listener = new MonitorPipeListener(new FakeMonitorService(Status, Incidents), _pipeName, clientAccount, NullLogger<MonitorPipeListener>.Instance);
        await listener.StartAsync(CancellationToken.None);

        return listener;
    }

    private async Task<T> CallOverPipeAsync<T>(Func<IMonitorService, Task<T>> call)
    {
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(5000);

        using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
        IMonitorService monitor = rpc.Attach<IMonitorService>();
        rpc.StartListening();

        return await call(monitor);
    }
}