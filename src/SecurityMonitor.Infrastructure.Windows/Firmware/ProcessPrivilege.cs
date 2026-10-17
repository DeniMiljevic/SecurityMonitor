using System.ComponentModel;
using System.Runtime.InteropServices;
using static SecurityMonitor.Infrastructure.Windows.Firmware.NativeMethods;

namespace SecurityMonitor.Infrastructure.Windows.Firmware;

internal static class ProcessPrivilege
{
    public static void Enable(string privilegeName)
    {
        if (!OpenProcessToken(GetCurrentProcess(), TokenAdjustPrivileges | TokenQuery, out var token))
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not open the process token.");

        using (token)
        {
            if (!LookupPrivilegeValueW(null, privilegeName, out var luid))
                throw new Win32Exception(Marshal.GetLastPInvokeError(), $"Unknown privilege '{privilegeName}'.");

            var privileges = new TokenPrivileges { PrivilegeCount = 1, Luid = luid, Attributes = SePrivilegeEnabled };
            var ok = AdjustTokenPrivileges(token, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero);

            var error = Marshal.GetLastPInvokeError();
            if (!ok || error == ErrorNotAllAssigned)
                throw new Win32Exception(error, $"The process does not have {privilegeName}. Run the app as administrator (highest privileges).");
        }
    }
}
