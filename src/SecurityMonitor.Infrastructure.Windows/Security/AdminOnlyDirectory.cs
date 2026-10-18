using System.Security.AccessControl;
using System.Security.Principal;

namespace SecurityMonitor.Infrastructure.Windows.Security;

public static class AdminOnlyDirectory
{
    public static void Apply(string path)
    {
        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        var system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);

        var security = new DirectorySecurity();
        security.SetOwner(administrators);
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(FullControl(system));
        security.AddAccessRule(FullControl(administrators));

        var directory = new DirectoryInfo(path);
        if (directory.Exists)
            directory.SetAccessControl(security);
        else
            directory.Create(security);
    }

    private static FileSystemAccessRule FullControl(SecurityIdentifier identity) =>
        new(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
}
