using System.Buffers.Binary;
using System.Text;

namespace SecurityMonitor.Infrastructure.Common.Firmware;

public sealed record UefiLoadOption(bool IsActive, string Description, string? FilePath, bool IsWindowsBootManager)
{
    private const uint LoadOptionActive = 0x00000001;
    private const string WindowsLoader = @"\EFI\Microsoft\Boot\bootmgfw.efi";

    // Layout (UEFI specification, EFI_LOAD_OPTION):
    //   UINT32  Attributes
    //   UINT16  FilePathListLength
    //   CHAR16  Description[]      UTF-16, ends with 0x0000
    //   BYTE    FilePathList[]     FilePathListLength bytes of device path nodes
    //   BYTE    OptionalData[]     the rest; Windows starts it with "WINDOWS"
    public static UefiLoadOption Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
            throw new FormatException("The boot entry is too short.");

        uint attributes = BinaryPrimitives.ReadUInt32LittleEndian(data);
        ushort filePathListLength = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);

        int descriptionStart = 6;
        int descriptionEnd = FindUtf16Terminator(data, descriptionStart);
        string description = Encoding.Unicode.GetString(data[descriptionStart..descriptionEnd]);

        int pathStart = descriptionEnd + 2;
        if (pathStart + filePathListLength > data.Length)
            throw new FormatException("The boot entry's device path is longer than the entry.");

        ReadOnlySpan<byte> devicePath = data.Slice(pathStart, filePathListLength);
        ReadOnlySpan<byte> optionalData = data[(pathStart + filePathListLength)..];

        string? filePath = FindFilePath(devicePath);
        bool isWindows = optionalData.StartsWith("WINDOWS"u8)
            || string.Equals(filePath, WindowsLoader, StringComparison.OrdinalIgnoreCase);

        return new UefiLoadOption((attributes & LoadOptionActive) != 0, description, filePath, isWindows);
    }

    private static int FindUtf16Terminator(ReadOnlySpan<byte> data, int start)
    {
        for (int i = start; i + 1 < data.Length; i += 2)
        {
            if (data[i] == 0 && data[i + 1] == 0)
                return i;
        }

        throw new FormatException("The boot entry's description is not terminated.");
    }

    private static string? FindFilePath(ReadOnlySpan<byte> devicePath)
    {
        int offset = 0;
        while (offset + 4 <= devicePath.Length)
        {
            byte type = devicePath[offset];
            byte subType = devicePath[offset + 1];
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(devicePath[(offset + 2)..]);

            if (length < 4 || offset + length > devicePath.Length)
                return null;

            if (type == 0x7F && subType == 0xFF)
                return null;

            if (type == 0x04 && subType == 0x04)
                return Encoding.Unicode.GetString(devicePath.Slice(offset + 4, length - 4)).TrimEnd('\0');

            offset += length;
        }

        return null;
    }
}