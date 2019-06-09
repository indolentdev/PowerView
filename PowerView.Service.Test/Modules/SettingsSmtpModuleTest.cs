using System.Globalization;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsSmtpModuleTest
  {
    private Mock<ISettingRepository> settingRepository;

    private Browser browser;

    private const string SmtpRoute = "/api/settings/smtp";

    [SetUp]
    public void SetUp()
    {
      settingRepository = new Mock<ISettingRepository>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsSmtpModule>();
        cfg.Dependency<ISettingRepository>(settingRepository.Object);
      });
    }

    [Test]
    public void GetSettingsNotAvailable()
    {
      // Arrange
      settingRepository.Setup(sr => sr.GetSmtpConfig()).Throws(new DomainConstraintException());

      // Act
      var response = browser.Get(SmtpRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestSmtpConfigDto>();
      Assert.That(json.server, Is.Null);
      Assert.That(json.port, Is.Null);
      Assert.That(json.user, Is.Null);
      Assert.That(json.auth, Is.Null);
    }

    [Test]
    public void GetSettings()
    {
      // Arrange
      var smtpConfig = new SmtpConfig("theServer", 1234, "theUser", "theAuth");
      settingRepository.Setup(sr => sr.GetSmtpConfig()).Returns(smtpConfig);

      // Act
      var response = browser.Get(SmtpRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestSmtpConfigDto>();
      Assert.That(json.server, Is.EqualTo(smtpConfig.Server));
      Assert.That(json.port, Is.EqualTo(smtpConfig.Port.ToString(CultureInfo.InvariantCulture)));
      Assert.That(json.user, Is.EqualTo(smtpConfig.User));
      Assert.That(json.auth, Is.EqualTo(smtpConfig.Auth));
    }

    [Test]
    public void PutSettings()
    {
      // Arrange
      var smtpConfigDto = new TestSmtpConfigDto { server = "theServer", port = "1234", user = "theUser", auth = "theAuth" };

      // Act
      var response = browser.Put(SmtpRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(smtpConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      settingRepository.Verify(sr => sr.UpsertSmtpConfig(
        It.Is<SmtpConfig>(x => x.Server == smtpConfigDto.server && x.Port == 1234 && x.User == smtpConfigDto.user && x.Auth == smtpConfigDto.auth )));
    }

    [Test]
    public void PutSettingsNoContent()
    {
      // Arrange

      // Act
      var response = browser.Put(SmtpRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("Server, Port, User or Auth properties absent, empty or invalid"));
    }

    [Test]
    [TestCase("abc")]
    [TestCase("0")]
    public void PutSettingsBadPort(string port)
    {
      // Arrange
      var smtpConfigDto = new TestSmtpConfigDto { server = "theServer", port = port, user = "theUser", auth = "theAuth" };

      // Act
      var response = browser.Put(SmtpRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(smtpConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("Server, Port, User or Auth properties absent, empty or invalid"));
    }

    internal class TestSmtpConfigDto
    {
      public string server { get; set; }
      public string port { get; set; }
      public string user { get; set; }
      public string auth { get; set; }
    }

  }
}

