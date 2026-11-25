using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SecurityMonitor.Infrastructure.Windows.Security;

public static class RestrictedPipe
{
    public static NamedPipeServerStream Create(string name, IEnumerable<SecurityIdentifier> clients, bool firstInstance)
    {
        var security = new PipeSecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User!, PipeAccessRights.FullControl, AccessControlType.Allow));

        foreach (SecurityIdentifier client in clients)
            security.AddAccessRule(new PipeAccessRule(client, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        PipeOptions options = firstInstance ? PipeOptions.Asynchronous | PipeOptions.FirstPipeInstance : PipeOptions.Asynchronous;

        return NamedPipeServerStreamAcl.Create(name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte, options, inBufferSize: 0, outBufferSize: 0, security);
    }
}
