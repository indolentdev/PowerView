using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using PowerView.Service.Mqtt;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsMqttModuleTest
  {
    private Mock<ISettingRepository> settingRepository;
    private Mock<IMqttPublisher> mqttPublisher;

    private Browser browser;

    private const string MqttRoute = "/api/settings/mqtt";
    private const string MqttTestRoute = "/api/settings/mqtt/test";

    [SetUp]
    public void SetUp()
    {
      settingRepository = new Mock<ISettingRepository>();
      mqttPublisher = new Mock<IMqttPublisher>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsMqttModule>();
        cfg.Dependency<ISettingRepository>(settingRepository.Object);
        cfg.Dependency<IMqttPublisher>(mqttPublisher.Object);
      });
    }

    [Test]
    public void GetSettings()
    {
      // Arrange
      var mqttConfig = new MqttConfig("theServer", 1234, true, "theClientId");
      settingRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

      // Act
      var response = browser.Get(MqttRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestMqttConfigDto>();
      Assert.That(json.publishEnabled, Is.EqualTo(mqttConfig.PublishEnabled));
      Assert.That(json.server, Is.EqualTo(mqttConfig.Server));
      Assert.That(json.port, Is.EqualTo(mqttConfig.Port.ToString(CultureInfo.InvariantCulture)));
    }

    [Test]
    public void PutSettings()
    {
      // Arrange
      var mqttConfigDto = new TestMqttConfigDto { publishEnabled = true, server = "theServer", port = "1234", clientId = "theClientId" };

      // Act
      var response = browser.Put(MqttRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(mqttConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      settingRepository.Verify(sr => sr.UpsertMqttConfig(
        It.Is<MqttConfig>(x => x.PublishEnabled == true && x.Server == mqttConfigDto.server && x.Port == 1234)));
    }

    [Test]
    public void PutSettingsNoContent()
    {
      // Arrange

      // Act
      var response = browser.Put(MqttRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("PublishEnabled, Server, Port or ClientId properties absent or empty"));
    }

    [Test]
    public void PutSettingsBadPort()
    {
      // Arrange
      var mqttConfigDto = new TestMqttConfigDto { publishEnabled = true, server = "theServer", port = "abc" };

      // Act
      var response = browser.Put(MqttRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(mqttConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("PublishEnabled, Server, Port or ClientId properties absent or empty"));
    }

    [Test]
    public void TestSettingsNoContent()
    {
      // Arrange

      // Act
      var response = browser.Put(MqttTestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("PublishEnabled, Server, Port or ClientId properties absent or empty"));
    }

    [Test]
    public void TestMqttNotAvailable()
    {
      // Arrange
      var mqttConfigDto = new TestMqttConfigDto { publishEnabled = true, server = "theServer", port = "1234", clientId = "theClientId" };
      mqttPublisher.Setup(mp => mp.Publish(It.IsAny<MqttConfig>(), It.IsAny<LiveReading[]>())).Throws(new MqttException());

      // Act
      var response = browser.Put(MqttTestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(mqttConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public void TestMqtt()
    {
      // Arrange
      var mqttConfigDto = new TestMqttConfigDto { publishEnabled = true, server = "theServer", port = "1234", clientId = "theClientId" };

      // Act
      var response = browser.Put(MqttTestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(mqttConfigDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      mqttPublisher.Verify(mp => mp.Publish(It.Is<MqttConfig>(x => x.Server == mqttConfigDto.server && x.Port == 1234),
                                           It.Is<ICollection<LiveReading>>(x => !x.Any())));
    }

    internal class TestMqttConfigDto
    {
      public string server { get; set; }
      public string port { get; set; }
      public bool publishEnabled { get; set; }
      public string clientId { get; set; }
    }

  }
}

