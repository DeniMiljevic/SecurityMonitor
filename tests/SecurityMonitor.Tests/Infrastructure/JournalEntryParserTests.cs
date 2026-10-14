using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Journal;

namespace SecurityMonitor.Tests.Infrastructure;

public class JournalEntryParserTests
{
    private const string LockScreenFailure = """{"SYSLOG_IDENTIFIER":"cinnamon-screensaver-pam-helper","__REALTIME_TIMESTAMP":"1791972671576615","_HOSTNAME":"deni-mint","SYSLOG_FACILITY":"10","MESSAGE":"pam_unix(cinnamon-screensaver:auth): authentication failure; logname=deni uid=1000 euid=1000 tty=:0 ruser= rhost=  user=deni"}""";

    [Fact]
    public void TryParse_LockScreenFailure_ReturnsEvent()
    {
        LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(LockScreenFailure);

        Assert.NotNull(loginEvent);
        Assert.Equal("cinnamon-screensaver", loginEvent.Service);
        Assert.Equal(":0", loginEvent.Terminal);
        Assert.Equal("deni", loginEvent.UserName);
        Assert.Null(loginEvent.SourceNetworkAddress);
        Assert.Equal("deni-mint", loginEvent.ComputerName);
    }

    [Fact]
    public void TryParse_Timestamp_IsConvertedFromMicroseconds()
    {
        // 1791972671576615 microseconds after 1970-01-01 = 2026-10-14 10:11:11.576615 UTC (12:11:11 in Zagreb)
        var expected = new DateTimeOffset(2026, 10, 14, 10, 11, 11, TimeSpan.Zero).AddTicks(5_766_150);

        LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(LockScreenFailure);

        Assert.NotNull(loginEvent);
        Assert.Equal(expected, loginEvent.Timestamp);
    }

    [Fact]
    public void TryParse_LoginScreenFailure_ReturnsLightdmEvent()
    {
        string line = Entry("pam_unix(lightdm:auth): authentication failure; logname= uid=0 euid=0 tty=:1 ruser= rhost=  user=deni");

        LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(line);

        Assert.NotNull(loginEvent);
        Assert.Equal("lightdm", loginEvent.Service);
        Assert.Equal(":1", loginEvent.Terminal);
    }

    [Fact]
    public void TryParse_RemoteHost_SetsSourceNetworkAddress()
    {
        string line = Entry("pam_unix(sshd:auth): authentication failure; logname= uid=0 euid=0 tty=ssh ruser= rhost=192.168.1.50  user=deni");

        LinuxFailedLoginEvent? loginEvent = JournalEntryParser.TryParse(line);

        Assert.NotNull(loginEvent);
        Assert.Equal("sshd", loginEvent.Service);
        Assert.Equal("192.168.1.50", loginEvent.SourceNetworkAddress);
    }

    [Theory]
    [InlineData("pam_unix(sudo:session): session opened for user root(uid=0) by deni(uid=1000)")]
    [InlineData("gkr-pam: unlocked login keyring")]
    [InlineData("")]
    public void TryParse_OtherMessage_ReturnsNull(string message)
    {
        Assert.Null(JournalEntryParser.TryParse(Entry(message)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[1, 2, 3]")]
    [InlineData("""{"MESSAGE": [112, 97, 109]}""")]
    public void TryParse_InvalidLine_ReturnsNull(string line)
    {
        Assert.Null(JournalEntryParser.TryParse(line));
    }

    private static string Entry(string message) =>
        $$"""{"__REALTIME_TIMESTAMP":"1791972671576615","_HOSTNAME":"deni-mint","MESSAGE":"{{message}}"}""";
}