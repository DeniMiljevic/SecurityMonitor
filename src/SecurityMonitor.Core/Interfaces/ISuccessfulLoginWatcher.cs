using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Core.Interfaces;

public interface ISuccessfulLoginWatcher : IDisposable
{
    event EventHandler<SuccessfulLoginEvent>? SuccessfulLoginDetected;

    void Start();

    void Stop();
}
