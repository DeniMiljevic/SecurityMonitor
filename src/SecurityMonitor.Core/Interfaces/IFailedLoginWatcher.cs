using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface IFailedLoginWatcher : IDisposable
{
    event EventHandler<FailedLoginEvent>? FailedLoginDetected;

    void Start();

    void Stop();
}
