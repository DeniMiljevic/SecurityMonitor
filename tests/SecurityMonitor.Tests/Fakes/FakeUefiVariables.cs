using SecurityMonitor.Infrastructure.Common.Firmware;

namespace SecurityMonitor.Tests.Fakes;

public sealed class FakeUefiVariables : IUefiVariables
{
    private readonly Dictionary<string, byte[]> _variables = [];

    public FakeUefiVariables WithBootOrder(params ushort[] order)
    {
        _variables["BootOrder"] = UefiBootOrder.ToBytes(order);
        return this;
    }

    public FakeUefiVariables WithEntry(ushort number, byte[] loadOption)
    {
        _variables[UefiBootOrder.VariableName(number)] = loadOption;
        return this;
    }

    public byte[]? ReadGlobal(string name) => _variables.GetValueOrDefault(name);
}
