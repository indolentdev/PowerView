using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mqtt;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsMqttControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ISettingRepository> settingRepository;
    private Mock<IMqttPublisher> mqttPublisher;

    [SetUp]
    public void SetUp()
    {
        settingRepository = new Mock<ISettingRepository>();
        mqttPublisher = new Mock<IMqttPublisher>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(settingRepository.Object);
                    sc.AddSingleton(mqttPublisher.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        httpClient?.Dispose();
        application?.Dispose();
    }

    [Test]
    public async Task GetSettings()
    {
        // Arrange
        var mqttConfig = new MqttConfig("theServer", 1234, true, "theClientId");
        settingRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

        // Act
        var response = await httpClient.GetAsync($"api/settings/mqtt");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestMqttConfigDto>();
        Assert.That(json.publishEnabled, Is.EqualTo(mqttConfig.PublishEnabled));
        Assert.That(json.server, Is.EqualTo(mqttConfig.Server));
        Assert.That(json.port, Is.EqualTo(mqttConfig.Port));
    }

    [Test]
    public async Task GetSettingsCallsRepository()
    {
        // Arrange
        var mqttConfig = new MqttConfig("theServer", 1234, true, "theClientId");
        settingRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

        // Act
        var response = await httpClient.GetAsync($"api/settings/mqtt");

        // Assert
        settingRepository.Verify(x => x.GetMqttConfig());
    }

    [Test]
    public async Task PutSettings()
    {
        // Arrange
        var mqttConfigDto = new { publishEnabled = true, server = "theServer", port = 1234, clientId = "theClientId" };
        var content = JsonContent.Create(mqttConfigDto);

        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        settingRepository.Verify(sr => sr.UpsertMqttConfig(
          It.Is<MqttConfig>(x => x.PublishEnabled == true && x.Server == mqttConfigDto.server && x.Port == 1234 && mqttConfigDto.clientId == x.ClientId)));
    }

    [Test]
    public async Task PutSettingsNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutSettingsBadPort()
    {
        // Arrange
        var mqttConfigDto = new { publishEnabled = true, server = "theServer", port = "abc", clientId = "ClId" };
        var content = JsonContent.Create(mqttConfigDto);

        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task TestSettingsNoContent()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public async Task TestMqttNotAvailable()
    {
        // Arrange
        var mqttConfigDto = new { publishEnabled = true, server = "theServer", port = 1234, clientId = "theClientId" };
        var content = JsonContent.Create(mqttConfigDto);
        mqttPublisher.Setup(mp => mp.Publish(It.IsAny<MqttConfig>(), It.IsAny<Reading[]>())).Throws(new MqttException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt/test", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task TestMqtt()
    {
        // Arrange
        var mqttConfigDto = new { publishEnabled = true, server = "theServer", port = 1234, clientId = "theClientId" };
        var content = JsonContent.Create(mqttConfigDto);

        // Act
        var response = await httpClient.PutAsync($"api/settings/mqtt/test", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        mqttPublisher.Verify(mp => mp.Publish(It.Is<MqttConfig>(x => x.Server == mqttConfigDto.server && x.Port == mqttConfigDto.port && x.ClientId == mqttConfigDto.clientId),
                                             It.Is<ICollection<Reading>>(x => !x.Any())));
    }

    internal class TestMqttConfigDto
    {
        public string server { get; set; }
        public int port { get; set; }
        public bool publishEnabled { get; set; }
        public string clientId { get; set; }
    }

}