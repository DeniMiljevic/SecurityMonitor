using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface IFailedLoginClassifier
{
    bool IsPhysicalAttempt(FailedLoginEvent loginEvent);
    AuthenticationMethod GetAuthenticationMethod(FailedLoginEvent loginEvent);
}
