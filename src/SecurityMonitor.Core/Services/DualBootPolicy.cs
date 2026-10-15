using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class DualBootPolicy
{
    private static readonly HashSet<string> LoginScreenServices = new(StringComparer.Ordinal)
    {
        "lightdm"
    };

    private const int FirstRegularUserId = 1000;

    private readonly bool _enabled;
    private readonly HashSet<string> _windowsUsers;

    public DualBootPolicy(DualBootOptions options)
    {
        _enabled = options.Enabled;
        _windowsUsers = new HashSet<string>(options.SwitchToWindowsUsers.Where(user => !string.IsNullOrWhiteSpace(user)).Select(user => user.Trim()), StringComparer.Ordinal);
    }

    public DualBootAction Evaluate(SuccessfulLoginEvent loginEvent)
    {
        if (!LoginScreenServices.Contains(loginEvent.Service))
            return DualBootAction.None;

        if (loginEvent.UserId is not int userId || userId < FirstRegularUserId)
            return DualBootAction.None;

        if (!_enabled)
            return DualBootAction.StayInLinux;

        return _windowsUsers.Contains(loginEvent.UserName) ? DualBootAction.SwitchToWindows : DualBootAction.StayInLinux;
    }
}
