using System.Security.AccessControl;
using System.Security.Principal;

namespace SecurityMonitor.Infrastructure.Windows.Security;

public static class RestrictedFile
{
    private static readonly SecurityIdentifier LocalSystem = new(WellKnownSidType.LocalSystemSid, null);
    private static readonly SecurityIdentifier Administrators = new(WellKnownSidType.BuiltinAdministratorsSid, null);

    public static void Apply(string path, IEnumerable<SecurityIdentifier> readers)
    {
        var security = new FileSecurity();

        security.SetOwner(Administrators);
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(LocalSystem, FileSystemRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(Administrators, FileSystemRights.FullControl, AccessControlType.Allow));

        foreach (SecurityIdentifier reader in readers)
            security.AddAccessRule(new FileSystemAccessRule(reader, FileSystemRights.Read, AccessControlType.Allow));

        new FileInfo(path).SetAccessControl(security);
    }
}
