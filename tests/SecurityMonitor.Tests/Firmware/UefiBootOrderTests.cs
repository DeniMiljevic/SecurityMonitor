using SecurityMonitor.Infrastructure.Common.Firmware;

namespace SecurityMonitor.Tests.Firmware;

public class UefiBootOrderTests
{
    [Fact]
    public void Parse_ReadsLittleEndianNumbers()
    {
        IReadOnlyList<ushort> order = UefiBootOrder.Parse([0x03, 0x00, 0x06, 0x00, 0x00, 0x00]);

        Assert.Equal(new ushort[] { 0x0003, 0x0006, 0x0000 }, order);
    }

    [Fact]
    public void ToBytes_IsTheReverseOfParse()
    {
        byte[] data = [0x06, 0x00, 0x03, 0x00, 0x0A, 0x01];

        Assert.Equal(data, UefiBootOrder.ToBytes(UefiBootOrder.Parse(data)));
    }

    [Fact]
    public void Parse_OddLength_Throws()
    {
        Assert.Throws<FormatException>(() => UefiBootOrder.Parse([0x03, 0x00, 0x06]));
    }

    [Fact]
    public void MoveToFront_KeepsAllOtherEntriesInOrder()
    {
        IReadOnlyList<ushort> order = UefiBootOrder.MoveToFront([0x0006, 0x0003, 0x0000, 0x0001], 0x0003);

        Assert.Equal(new ushort[] { 0x0003, 0x0006, 0x0000, 0x0001 }, order);
    }

    [Fact]
    public void MoveToFront_UnknownEntry_Throws()
    {
        Assert.Throws<ArgumentException>(() => UefiBootOrder.MoveToFront([0x0006, 0x0003], 0x0009));
    }

    [Fact]
    public void VariableName_IsFourHexDigits()
    {
        Assert.Equal("Boot000A", UefiBootOrder.VariableName(0x000A));
    }
}