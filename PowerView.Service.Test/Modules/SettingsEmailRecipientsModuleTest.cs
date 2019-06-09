using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsEmailRecipientsModuleTest
  {
    private Mock<IEmailRecipientRepository> emailRecipientRepository;
    private Mock<ITranslation> translation;
    private Mock<IMailMediator> mailMediator;

    private Browser browser;

    private const string EmailRecipientsRoute = "/api/settings/emailrecipients";

    [SetUp]
    public void SetUp()
    {
      emailRecipientRepository = new Mock<IEmailRecipientRepository>();
      translation = new Mock<ITranslation>();
      mailMediator = new Mock<IMailMediator>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsEmailRecipientsModule>();
        cfg.Dependency<IEmailRecipientRepository>(emailRecipientRepository.Object);
        cfg.Dependency<ITranslation>(translation.Object);
        cfg.Dependency<IMailMediator>(mailMediator.Object);
      });
    }

    [Test]
    public void GetEmailRecipients()
    {
      // Arrange
      var er1 = new EmailRecipient("Hugo", "a@b.com");
      var er2 = new EmailRecipient("Hansi", "p@q.com");
      var emailRecipients = new[] { er1, er2 };
      emailRecipientRepository.Setup(err => err.GetEmailRecipients()).Returns(emailRecipients);

      // Act
      var response = browser.Get(EmailRecipientsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestEmailRecipientSetDto>();
      Assert.That(json.items, Is.Not.Null);
      Assert.That(json.items.Length, Is.EqualTo(2));
      AssertEmailRecipient(er1, json.items.First());
      AssertEmailRecipient(er2, json.items.Last());
    }

    private static void AssertEmailRecipient(EmailRecipient expected, TestEmailRecipientDto actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.name, Is.EqualTo(expected.Name));
      Assert.That(actual.emailAddress, Is.EqualTo(expected.EmailAddress));
    }

    [Test]
    public void AddEmailRecipient()
    {
      // Arrange
      var emailRecipient = new { Name = "The Name", EmailAddress = "a@b.com" };

      // Act
      var response = browser.Post(EmailRecipientsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(emailRecipient);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      emailRecipientRepository.Verify(err => err.AddEmailRecipient(
        It.Is<EmailRecipient>(x => x.Name == emailRecipient.Name && x.EmailAddress == emailRecipient.EmailAddress)));
    }

    [Test]
    public void AddEmailRecipientNoContent()
    {
      // Arrange

      // Act
      var response = browser.Post(EmailRecipientsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("Name or EmailAddress properties absent or empty"));
    }

    [Test]
    public void AddEmailRecipientBadEmailAddress()
    {
      // Arrange
      var emailRecipient = new { Name = "The Name", EmailAddress = "BadEmailAddress" };

      // Act
      var response = browser.Post(EmailRecipientsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(emailRecipient);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("EmailAddress has invalid format"));
    }

    [Test]
    public void AddEmailRecipientEmailAddressAlreadyExists()
    {
      // Arrange
      var emailRecipient = new { Name = "The Name", EmailAddress = "a@b.com" };
      emailRecipientRepository.Setup(err => err.AddEmailRecipient(It.IsAny<EmailRecipient>())).Throws(new DataStoreUniqueConstraintException());

      // Act
      var response = browser.Post(EmailRecipientsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(emailRecipient);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
      Assert.That(response.Body.AsString(), Contains.Substring("EmailAddress already exists"));
    }

    [Test]
    public void DeleteEmailRecipient()
    {
      // Arrange
      const string emailAddress = "a@b.com";

      // Act
      var response = browser.Delete(EmailRecipientsRoute + "/" + emailAddress, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      emailRecipientRepository.Verify(err => err.DeleteEmailRecipient(emailAddress));
    }

    [Test]
    public void TestEmailRecipientNotFound()
    {
      // Arrange
      const string emailAddress = "a@b.com";

      // Act
      var response = browser.Put(EmailRecipientsRoute + "/" + emailAddress + "/test", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
      emailRecipientRepository.Verify(err => err.GetEmailRecipient(emailAddress));
    }

    [Test]
    public void TestEmailRecipient()
    {
      // Arrange
      const string emailAddress = "a@b.com";
      var emailRecipient = new EmailRecipient("The Name", emailAddress);
      emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
      const string subject = "The Subject";
      translation.Setup(t => t.Get(ResId.MailSubjectTest)).Returns(subject);
      const string message = "The Message";
      translation.Setup(t => t.Get(ResId.MailMessageTest)).Returns(message);

      // Act
      var response = browser.Put(EmailRecipientsRoute + "/" + emailAddress + "/test", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      translation.Verify(t => t.Get(ResId.MailSubjectTest));
      translation.Verify(t => t.Get(ResId.MailMessageTest));
      mailMediator.Verify(mm => mm.SendEmail(emailRecipient, subject, message));
    }

    [Test]
    public void TestEmailRecipientConnectError()
    {
      // Arrange
      const string emailAddress = "a@b.com";
      var emailRecipient = new EmailRecipient("The Name", emailAddress);
      emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
      mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Throws(new ConnectMailerException());

      // Act
      var response = browser.Put(EmailRecipientsRoute + "/" + emailAddress + "/test", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.GatewayTimeout));
    }

    [Test]
    public void TestEmailRecipientAuthenticateError()
    {
      // Arrange
      const string emailAddress = "a@b.com";
      var emailRecipient = new EmailRecipient("The Name", emailAddress);
      emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
      mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Throws(new AuthenticateMailerException());

      // Act
      var response = browser.Put(EmailRecipientsRoute + "/" + emailAddress + "/test", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)567));
    }

    [Test]
    public void TestEmailRecipientError()
    {
      // Arrange
      const string emailAddress = "a@b.com";
      var emailRecipient = new EmailRecipient("The Name", emailAddress);
      emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
      mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Throws(new MailerException());

      // Act
      var response = browser.Put(EmailRecipientsRoute + "/" + emailAddress + "/test", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    internal class TestEmailRecipientSetDto
    {
      public TestEmailRecipientDto[] items { get; set; }
    }

    internal class TestEmailRecipientDto
    {
      public string name { get; set; }
      public string emailAddress { get; set; }
    }

  }
}

