using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Services;

public sealed class FailedLoginClassifier
{
    public const int FailedLogonEventId = 4625;

    private const int Interactive = 2;
    private const int Unlock = 7;
    private const int CachedInteractive = 11;

    private const uint WrongPassword = 0xC000006A;
    private const uint WrongPin = 0xC0000380;

    public bool IsPhysicalAttempt(FailedLoginEvent loginEvent)
    {
        if (loginEvent.EventId != FailedLogonEventId)
            return false;

        bool isConsoleLogon = loginEvent.LogonType is Interactive or Unlock or CachedInteractive;

        if (!isConsoleLogon)
            return false;

        return IsLocalSource(loginEvent.SourceNetworkAddress);
    }

    public AuthenticationMethod GetAuthenticationMethod(FailedLoginEvent loginEvent) =>
        loginEvent.FailureSubStatus switch
        {
            WrongPin => AuthenticationMethod.Pin,
            WrongPassword => AuthenticationMethod.Password,
            _ => AuthenticationMethod.Unknown
        };

    private static bool IsLocalSource(string? address) =>
        string.IsNullOrWhiteSpace(address) || address is "-" or "127.0.0.1" or "::1";
}
