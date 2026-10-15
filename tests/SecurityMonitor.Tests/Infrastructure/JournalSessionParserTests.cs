using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Linux.Journal;

namespace SecurityMonitor.Tests.Infrastructure;

public class JournalSessionParserTests
{
    [Fact]
    public void TryParseSessionOpened_LoginScreen_ReturnsEvent()
    {
        string line = Entry("pam_unix(lightdm:session): session opened for user deni(uid=1000) by (uid=0)");

        SuccessfulLoginEvent? loginEvent = JournalEntryParser.TryParseSessionOpened(line);

        Assert.NotNull(loginEvent);
        Assert.Equal("lightdm", loginEvent.Service);
        Assert.Equal("deni", loginEvent.UserName);
        Assert.Equal(1000, loginEvent.UserId);
        Assert.Equal("deni-dualboot", loginEvent.ComputerName);
    }

    [Fact]
    public void TryParseSessionOpened_AdminLogin_ReturnsAdmin()
    {
        string line = Entry("pam_unix(lightdm:session): session opened for user admin(uid=1001) by (uid=0)");

        SuccessfulLoginEvent? loginEvent = JournalEntryParser.TryParseSessionOpened(line);

        Assert.NotNull(loginEvent);
        Assert.Equal("admin", loginEvent.UserName);
        Assert.Equal(1001, loginEvent.UserId);
    }

    [Theory]
    [InlineData("pam_unix(lightdm-greeter:session): session opened for user lightdm(uid=113) by (uid=0)", "lightdm-greeter", "lightdm", 113)]
    [InlineData("pam_unix(systemd-user:session): session opened for user deni(uid=1000) by deni(uid=0)", "systemd-user", "deni", 1000)]
    [InlineData("pam_unix(sudo:session): session opened for user root(uid=0) by deni(uid=1000)", "sudo", "root", 0)]
    public void TryParseSessionOpened_OtherSessions_AreParsedAsIs(string message, string service, string user, int uid)
    {
        SuccessfulLoginEvent? loginEvent = JournalEntryParser.TryParseSessionOpened(Entry(message));

        Assert.NotNull(loginEvent);
        Assert.Equal(service, loginEvent.Service);
        Assert.Equal(user, loginEvent.UserName);
        Assert.Equal(uid, loginEvent.UserId);
    }

    [Theory]
    [InlineData("pam_unix(lightdm:auth): authentication failure; logname= uid=0 euid=0 tty=:1 ruser= rhost=  user=deni")]
    [InlineData("pam_unix(lightdm:session): session closed for user deni")]
    [InlineData("gkr-pam: unlocked login keyring")]
    [InlineData("")]
    public void TryParseSessionOpened_OtherMessage_ReturnsNull(string message)
    {
        Assert.Null(JournalEntryParser.TryParseSessionOpened(Entry(message)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[1, 2, 3]")]
    public void TryParseSessionOpened_InvalidLine_ReturnsNull(string line)
    {
        Assert.Null(JournalEntryParser.TryParseSessionOpened(line));
    }

    private static string Entry(string message) =>
        $$"""{"__REALTIME_TIMESTAMP":"1791972671576615","_HOSTNAME":"deni-dualboot","MESSAGE":"{{message}}"}""";
}