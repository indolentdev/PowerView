using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SmtpServer.Protocol;
using PowerView.Model;
using PowerView.Service.Mailer;

namespace PowerView.Service.Test.Mailer
{
  [TestFixture]
  public class SmtpMailerTest
  {
    private TestSmtpServer smtpServer;
    private readonly string smtpUser = "TheSmtpUser@b.com";
    private readonly string smtpPw = "TheSmtpPw";
    private readonly string smtpName = "PowerView";

    [SetUp]
    public void SetUp()
    {
      smtpServer = new TestSmtpServer();
    }

    [TearDown]
    public Task TearDown()
    {
      return smtpServer.Shutdown();
    }

    [Test]
    public void SendThrows()
    {
      // Arrange
      var target = CreateTarget();
      var smtpConfig = GetSmtpConfig();
      var emailRecipient = new EmailRecipient("toname", "a@b.com");

      // Act & Asssert
      Assert.That(() => target.Send(null, emailRecipient, "subject", "message"), Throws.ArgumentNullException);
      Assert.That(() => target.Send(smtpConfig, null, "subject", "message"), Throws.ArgumentNullException);
    }

    [Test]
    [TestCase("BadServerName", 10)]
    public void SendBadServerEndpointThrows(string server, int port)
    {
      // Arrange
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(server, port), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<ConnectMailerException>());
    }

    [Test]
    public void SendServerTlsUnsupportedThrows()
    {
      // Arrange
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<ConnectMailerException>());
    }

    [Test]
    public void SendServerCertificateUnsupportedOnClientSideThrows()
    {
      // Arrange
      smtpServer.RequireTls("unknownSiteName"); // The certificate site name doesn't match the DNS name
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<ConnectMailerException>());
    }

    [Test]
    public void SendServerUserAuthenticationUnsupportedThrows()
    {
      // Arrange
      smtpServer.RequireTls(smtpServer.ServerName);
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<AuthenticateMailerException>());
    }

    [Test]
    public void SmtpUserNotAuthenticatedThrows()
    {
      // Arrange
      smtpServer.RequireTls(smtpServer.ServerName);
      smtpServer.RequireUser("baduser", smtpPw);
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<AuthenticateMailerException>());
    }

    [Test]
    public void SmtpPasswordNotAuthenticatedThrows()
    {
      // Arrange
      smtpServer.RequireTls(smtpServer.ServerName);
      smtpServer.RequireUser(smtpUser, "badPw");
      smtpServer.Start();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<AuthenticateMailerException>());
    }

    [Test]
    public void ToAddressMailboxUnavailableThrows()
    {
      // Arrange
      smtpServer.RequireTls(smtpServer.ServerName); 
      smtpServer.RequireUser(smtpUser, smtpPw);
      smtpServer.Start();
      var target = CreateTarget();
      smtpServer.SetNextSendSmtpResponse(SmtpResponse.MailboxUnavailable);

      // Act & Assert
      Assert.That(() => target.Send(GetSmtpConfig(), new EmailRecipient("toname", "a@b.com"), "subject", "message"), Throws.TypeOf<MailerException>());
    }

    [Test]
    public void Send()
    {
      // Arrange
      smtpServer.RequireTls(smtpServer.ServerName);
      smtpServer.RequireUser(smtpUser, smtpPw);
      smtpServer.Start();
      var target = CreateTarget();
      const string toName = "toname";
      const string toAddr = "a@b.com";
      var to = new EmailRecipient(toName, toAddr);
      const string subject = "the Subject";
      const string message = "the Message";

      // Act & Assert
      var frm = target.Send(GetSmtpConfig(), to, subject, message);

      // Assert
      Assert.That(frm, Is.Not.Null);
      Assert.That(frm.Name, Is.EqualTo(smtpName));
      Assert.That(frm.EmailAddress, Is.EqualTo(smtpUser));

      Assert.That(smtpServer.Messages, Has.Count.EqualTo(1));
      var mimeMessage = smtpServer.Messages[0];
      AssertMailboxAddress(smtpUser, "PowerView", mimeMessage.From);
      AssertMailboxAddress(toAddr, toName, mimeMessage.To);
      Assert.That(mimeMessage.Subject, Is.EqualTo(subject));
      Assert.That(mimeMessage.TextBody, Is.EqualTo(message));

      AssertMailboxAddress(toAddr, toName, mimeMessage.ReplyTo);
      Assert.That(mimeMessage.Cc, Is.Empty);
      Assert.That(mimeMessage.Bcc, Is.Empty);
      Assert.That(mimeMessage.Attachments, Is.Empty);
    }

    private void AssertMailboxAddress(string expectedAddress, string expectedName, MimeKit.InternetAddressList addressList)
    {
      Assert.That(addressList, Has.Count.EqualTo(1));
      var addr = (MimeKit.MailboxAddress)addressList.First();
      Assert.That(addr.Name, Is.EqualTo(expectedName));
      Assert.That(addr.Address, Is.EqualTo(expectedAddress));
    }

    private SmtpMailer CreateTarget()
    {
      return new SmtpMailer(new NullLogger<SmtpMailer>());
    }

    private SmtpConfig GetSmtpConfig(string serverName = null, int? port = null)
    {
      var serverNameLocal = serverName ?? smtpServer.ServerName;
      var portLocal = (ushort)(port ?? smtpServer.Port);

      return new SmtpConfig(serverNameLocal, portLocal, smtpUser, smtpPw);
    }

  }
}
