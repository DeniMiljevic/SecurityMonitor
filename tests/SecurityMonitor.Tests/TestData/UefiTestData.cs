using System.Buffers.Binary;
using System.Text;

namespace SecurityMonitor.Tests.TestData;

public static class UefiTestData
{
    public static readonly byte[] WindowsOptionalData = Convert.FromHexString("57494e444f57530001000000880000007800000042004300440030004200");

    public static byte[] LoadOption(string description, string filePath, bool active = true, byte[]? optionalData = null)
    {
        byte[] hardDrive = Node(0x04, 0x01, new byte[38]);
        byte[] file = Node(0x04, 0x04, Encoding.Unicode.GetBytes(filePath + "\0"));
        byte[] end = Node(0x7F, 0xFF, []);
        byte[] devicePath = [.. hardDrive, .. file, .. end];

        var header = new byte[6];
        BinaryPrimitives.WriteUInt32LittleEndian(header, active ? 1u : 0u);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), (ushort)devicePath.Length);

        return [.. header, .. Encoding.Unicode.GetBytes(description + "\0"), .. devicePath, .. optionalData ?? []];
    }

    private static byte[] Node(byte type, byte subType, byte[] payload)
    {
        var node = new byte[4 + payload.Length];
        node[0] = type;
        node[1] = subType;
        BinaryPrimitives.WriteUInt16LittleEndian(node.AsSpan(2), (ushort)node.Length);
        payload.CopyTo(node, 4);
        return node;
    }
}