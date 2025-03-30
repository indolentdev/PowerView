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

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsSmtpControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ISettingRepository> settingRepository;

    [SetUp]
    public void SetUp()
    {
        settingRepository = new Mock<ISettingRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(settingRepository.Object);
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
    public async Task GetSettingsNotAvailable()
    {
        // Arrange
        settingRepository.Setup(sr => sr.GetSmtpConfig()).Throws(new DomainConstraintException());

        // Act
        var response = await httpClient.GetAsync($"api/settings/smtp");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestSmtpConfigDto>();
        Assert.That(json.server, Is.Null);
        Assert.That(json.port, Is.Null);
        Assert.That(json.user, Is.Null);
        Assert.That(json.auth, Is.Null);
        Assert.That(json.email, Is.Null);
    }

    [Test]
    public async Task GetSettings()
    {
        // Arrange
        var smtpConfig = new SmtpConfig("theServer", 1234, "theUser", "theAuth", "a@b.com");
        settingRepository.Setup(sr => sr.GetSmtpConfig()).Returns(smtpConfig);

        // Act
        var response = await httpClient.GetAsync($"api/settings/smtp");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestSmtpConfigDto>();
        Assert.That(json.server, Is.EqualTo(smtpConfig.Server));
        Assert.That(json.port, Is.EqualTo(smtpConfig.Port));
        Assert.That(json.user, Is.EqualTo(smtpConfig.User));
        Assert.That(json.auth, Is.EqualTo(smtpConfig.Auth));
        Assert.That(json.email, Is.EqualTo(smtpConfig.Email));
    }

    [Test]
    public async Task PutSettings()
    {
        // Arrange
        var smtpConfigDto = new TestSmtpConfigDto { server = "theServer", port = 1234, user = "theUser", auth = "theAuth", email = "a@b.com" };
        var content = JsonContent.Create(smtpConfigDto);

        // Act
        var response = await httpClient.PutAsync($"api/settings/smtp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        settingRepository.Verify(sr => sr.UpsertSmtpConfig(
          It.Is<SmtpConfig>(x => x.Server == smtpConfigDto.server && x.Port == 1234 && x.User == smtpConfigDto.user && x.Auth == smtpConfigDto.auth && x.Email == smtpConfigDto.email)));
    }

    [Test]
    public async Task PutSettingsNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PutAsync($"api/settings/smtp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutSettingsBadPort()
    {
        // Arrange
        var smtpConfigDto = new { server = "theServer", port = "abc", user = "theUser", auth = "theAuth" };
        var content = JsonContent.Create(smtpConfigDto);

        // Act
        var response = await httpClient.PutAsync($"api/settings/smtp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    internal class TestSmtpConfigDto
    {
        public string server { get; set; }
        public ushort? port { get; set; }
        public string user { get; set; }
        public string auth { get; set; }
        public string email { get; set; }
    }

}
