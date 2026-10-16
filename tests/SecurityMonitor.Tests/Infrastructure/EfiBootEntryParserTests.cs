using SecurityMonitor.Infrastructure.Linux.Boot;

namespace SecurityMonitor.Tests.Infrastructure;

public class EfiBootEntryParserTests
{
    // Real efibootmgr output from the DualBootTest VM (device paths shortened).
    public const string DualBootOutput =
        "BootCurrent: 0003\n" +
        "Timeout: 0 seconds\n" +
        "BootOrder: 0003,0004,0000,0001,0002\n" +
        "Boot0000* UiApp\tFvVol(7cb8bdc9-f8eb-4f34-aaea-3ee4af6516a1)/FvFile(462caa21-7614-4503-836e-8ab6f4662331)\n" +
        "Boot0001* UEFI VBOX CD-ROM VB1-1a2b3c4d \tPciRoot(0x0)/Pci(0xd,0x0)/Sata(1,65535,0)\n" +
        "Boot0002* UEFI VBOX HARDDISK VB0f9d0959-ad21edfa \tPciRoot(0x0)/Pci(0xd,0x0)/Sata(0,65535,0)\n" +
        "Boot0003* Ubuntu\tHD(1,GPT,8a245da5-caec-41ea-b902-ba2475d6aab1,0x800,0x64000)/File(\\EFI\\ubuntu\\shimx64.efi)\n" +
        "Boot0004* Windows Boot Manager\tHD(1,GPT,8a245da5-caec-41ea-b902-ba2475d6aab1,0x800,0x64000)/File(\\EFI\\Microsoft\\Boot\\bootmgfw.efi)57494e444f5753\n";

    [Fact]
    public void ParseEntries_RealOutput_ReturnsAllBootEntries()
    {
        IReadOnlyList<EfiBootEntry> entries = EfiBootEntryParser.ParseEntries(DualBootOutput);

        Assert.Equal(5, entries.Count);
        Assert.Contains(new EfiBootEntry("0003", "Ubuntu", true), entries);
        Assert.Contains(new EfiBootEntry("0004", "Windows Boot Manager", true), entries);
    }

    [Fact]
    public void ParseEntries_HeaderLines_AreNotEntries()
    {
        IReadOnlyList<EfiBootEntry> entries = EfiBootEntryParser.ParseEntries("BootCurrent: 0003\nBootNext: 0004\nBootOrder: 0003,0004\n");

        Assert.Empty(entries);
    }

    [Fact]
    public void ParseEntries_InactiveEntry_IsMarkedInactive()
    {
        IReadOnlyList<EfiBootEntry> entries = EfiBootEntryParser.ParseEntries("Boot000a  Old Windows\tHD(...)\r\n");

        EfiBootEntry entry = Assert.Single(entries);
        Assert.Equal("000A", entry.Number);
        Assert.Equal("Old Windows", entry.Label);
        Assert.False(entry.IsActive);
    }

    [Fact]
    public void Parse_RealOutput_ReadsBootCurrentAndBootOrder()
    {
        EfiBootState state = EfiBootEntryParser.Parse(DualBootOutput);

        Assert.Equal("0003", state.BootCurrent);
        Assert.Equal(new[] { "0003", "0004", "0000", "0001", "0002" }, state.BootOrder);
        Assert.Equal(5, state.Entries.Count);
    }

    [Fact]
    public void Parse_WithoutHeaderLines_HasNoBootCurrentOrOrder()
    {
        EfiBootState state = EfiBootEntryParser.Parse("Boot0003* Ubuntu\tHD(...)\n");

        Assert.Null(state.BootCurrent);
        Assert.Empty(state.BootOrder);
    }
}