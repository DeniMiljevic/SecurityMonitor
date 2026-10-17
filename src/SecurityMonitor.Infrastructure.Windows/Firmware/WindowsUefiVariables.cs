using System.ComponentModel;
using System.Runtime.InteropServices;
using SecurityMonitor.Infrastructure.Common.Firmware;
using static SecurityMonitor.Infrastructure.Windows.Firmware.NativeMethods;

namespace SecurityMonitor.Infrastructure.Windows.Firmware;

public sealed class WindowsUefiVariables : IUefiVariables
{
    private const string GlobalVariableGuid = "{8BE4DF61-93CA-11D2-AA0D-00E098032B8C}";
    private const int InitialBufferSize = 1024;
    private const int MaxBufferSize = 64 * 1024;

    private readonly Lock _lock = new();
    private bool _privilegeEnabled;

    public byte[]? ReadGlobal(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsurePrivilegeEnabled();

        for (var size = InitialBufferSize; ; size *= 2)
        {
            var buffer = new byte[size];
            var length = GetFirmwareEnvironmentVariableExW(name, GlobalVariableGuid, buffer, (uint)size, out _);
            if (length > 0)
                return buffer[..(int)length];

            var error = Marshal.GetLastPInvokeError();
            if (error == ErrorEnvVarNotFound)
                return null;

            if (error != ErrorInsufficientBuffer || size >= MaxBufferSize)
                throw new Win32Exception(error, $"Could not read UEFI variable '{name}'.");
        }
    }

    private void EnsurePrivilegeEnabled()
    {
        lock (_lock)
        {
            if (_privilegeEnabled)
                return;

            ProcessPrivilege.Enable(SeSystemEnvironmentName);
            _privilegeEnabled = true;
        }
    }
}