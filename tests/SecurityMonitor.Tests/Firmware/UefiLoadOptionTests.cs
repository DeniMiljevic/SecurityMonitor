using SecurityMonitor.Infrastructure.Common.Firmware;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Firmware;

public class UefiLoadOptionTests
{
    [Fact]
    public void Parse_UbuntuEntry_ReadsDescriptionAndPath()
    {
        UefiLoadOption option = UefiLoadOption.Parse(UefiTestData.LoadOption("Ubuntu", @"\EFI\ubuntu\shimx64.efi"));

        Assert.True(option.IsActive);
        Assert.Equal("Ubuntu", option.Description);
        Assert.Equal(@"\EFI\ubuntu\shimx64.efi", option.FilePath);
        Assert.False(option.IsWindowsBootManager);
    }

    [Fact]
    public void Parse_EntryWrittenByWindows_IsWindowsBootManager()
    {
        byte[] data = UefiTestData.LoadOption("Windows Boot Manager", @"\EFI\Microsoft\Boot\bootmgfw.efi", optionalData: UefiTestData.WindowsOptionalData);

        UefiLoadOption option = UefiLoadOption.Parse(data);

        Assert.True(option.IsWindowsBootManager);
    }

    [Fact]
    public void Parse_EntryCreatedByHand_IsRecognisedByItsLoaderPath()
    {
        byte[] data = UefiTestData.LoadOption("Windows Boot Manager", @"\EFI\Microsoft\Boot\bootmgfw.efi");

        Assert.True(UefiLoadOption.Parse(data).IsWindowsBootManager);
    }

    [Fact]
    public void Parse_InactiveEntry_IsNotActive()
    {
        UefiLoadOption option = UefiLoadOption.Parse(UefiTestData.LoadOption("Old", @"\EFI\old\grubx64.efi", active: false));

        Assert.False(option.IsActive);
    }

    [Fact]
    public void Parse_TooShort_Throws()
    {
        Assert.Throws<FormatException>(() => UefiLoadOption.Parse([1, 0, 0, 0]));
    }

    [Fact]
    public void Parse_DevicePathLongerThanEntry_Throws()
    {
        byte[] data = UefiTestData.LoadOption("Ubuntu", @"\EFI\ubuntu\shimx64.efi");

        Assert.Throws<FormatException>(() => UefiLoadOption.Parse(data.AsSpan(0, data.Length - 10)));
    }
}