namespace SecurityMonitor.Infrastructure.Common.Firmware;

public interface IUefiVariables
{
    byte[]? ReadGlobal(string name);
}
