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
    private static readonly string CurrentUser = WindowsIdentity.GetCurrent().Name;

    private readonly string _pipeName = $"SecurityMonitor.Tests.{Guid.NewGuid():N}";

    [Fact]
    public async Task AllowedAccount_GetsTheStatus()
    {
        using MonitorPipeListener listener = await StartListenerAsync(clientAccount: CurrentUser);

        Assert.Equal(Status, await GetStatusOverPipeAsync());

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

        // The listener gives up instead of waiting for clients on someone else's pipe.
        await listener.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private async Task<MonitorPipeListener> StartListenerAsync(string clientAccount)
    {
        var listener = new MonitorPipeListener(new FakeMonitorService(Status), _pipeName, clientAccount, NullLogger<MonitorPipeListener>.Instance);
        await listener.StartAsync(CancellationToken.None);

        return listener;
    }

    private async Task<MonitorStatus> GetStatusOverPipeAsync()
    {
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(5000);

        // Same handler and formatter as MonitorClient in the API.
        using var rpc = new JsonRpc(new HeaderDelimitedMessageHandler(pipe, new SystemTextJsonFormatter()));
        IMonitorService monitor = rpc.Attach<IMonitorService>();
        rpc.StartListening();

        return await monitor.GetStatusAsync(CancellationToken.None);
    }
}
