using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class LinuxFailedLoginClassifier : IFailedLoginClassifier
{
    private static readonly HashSet<string> PhysicalServices = new(StringComparer.Ordinal)
    {
        "lightdm",
        "cinnamon-screensaver"
    };

    public bool IsPhysicalAttempt(FailedLoginEvent loginEvent)
    {
        if (loginEvent is not LinuxFailedLoginEvent linuxEvent)
            return false;

        if (!PhysicalServices.Contains(linuxEvent.Service))
            return false;

        return string.IsNullOrWhiteSpace(linuxEvent.SourceNetworkAddress);
    }

    public AuthenticationMethod GetAuthenticationMethod(FailedLoginEvent loginEvent) =>
        loginEvent is LinuxFailedLoginEvent ? AuthenticationMethod.Password : AuthenticationMethod.Unknown;
}
