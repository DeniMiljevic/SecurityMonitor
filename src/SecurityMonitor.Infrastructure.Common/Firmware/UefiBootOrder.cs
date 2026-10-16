using System.Buffers.Binary;

namespace SecurityMonitor.Infrastructure.Common.Firmware;

public static class UefiBootOrder
{
    public static IReadOnlyList<ushort> Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length % 2 != 0)
            throw new FormatException("BootOrder must contain whole 16-bit numbers.");

        var order = new ushort[data.Length / 2];
        for (int i = 0; i < order.Length; i++)
            order[i] = BinaryPrimitives.ReadUInt16LittleEndian(data[(i * 2)..]);

        return order;
    }

    public static byte[] ToBytes(IReadOnlyList<ushort> order)
    {
        var data = new byte[order.Count * 2];
        for (int i = 0; i < order.Count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(i * 2), order[i]);

        return data;
    }

    public static IReadOnlyList<ushort> MoveToFront(IReadOnlyList<ushort> order, ushort entry)
    {
        if (!order.Contains(entry))
            throw new ArgumentException($"Boot{entry:X4} is not in the boot order.", nameof(entry));

        return [entry, .. order.Where(number => number != entry)];
    }

    public static string VariableName(ushort entry) => $"Boot{entry:X4}";
}