using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsApplicationModuleTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ILocationContext> locationContext;

    [SetUp]
    public void SetUp()
    {
        locationContext = new Mock<ILocationContext>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(locationContext.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task Get()
    {
        // Arrange
        var cultureInfo = new CultureInfo("de-DE");
        locationContext.Setup(lc => lc.CultureInfo).Returns(cultureInfo);
        const string timeZoneDisplayName = "TZ Name";
        locationContext.Setup(lc => lc.GetTimeZoneDisplayName()).Returns(timeZoneDisplayName);


        // Act
        var response = await httpClient.GetAsync($"api/settings/application");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestApplicationDto>();
        Assert.That(json, Is.Not.Null);
        Assert.That(json.version, Is.Not.Null);
        Assert.That(json.culture, Is.EqualTo(cultureInfo.NativeName));
        Assert.That(json.timeZone, Is.EqualTo(timeZoneDisplayName));
    }

    internal class TestApplicationDto
    {
        public string version { get; set; }
        public string culture { get; set; }
        public string timeZone { get; set; }
    }

}
