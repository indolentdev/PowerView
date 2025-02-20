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
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.IntegrationTest;

public class DeviceOnDemandControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IDisconnectControlCache> disconnectControlCache;

    [SetUp]
    public void Setup()
    {
        disconnectControlCache = new Mock<IDisconnectControlCache>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(disconnectControlCache.Object);
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
    public async Task GetOnDemand_CallsDisconnectControlCache()
    {
        // Arrange
        SetupDisconnectControlCacheGetOutputStatus();

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=theLabel");

        // Assert
        disconnectControlCache.Verify(x => x.GetOutputStatus("theLabel"));
    }

    [Test]
    public async Task GetOnDemand()
    {
        // Arrange
        var seriesName1 = new SeriesName("lbl", "0.1.96.3.10.255");
        var connected1 = true;
        var seriesName2 = new SeriesName("lbl", "0.2.96.3.10.255");
        var connected2 = false;
        SetupDisconnectControlCacheGetOutputStatus((seriesName1, connected1), (seriesName2, connected2));

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=theLabel");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestOnDemandSetDto>();
        Assert.That(json.items.Length, Is.EqualTo(2));
        Assert.That(json.items[0].label, Is.EqualTo("lbl"));
        Assert.That(json.items[0].obisCode, Is.EqualTo("0.1.96.3.10.255"));
        Assert.That(json.items[0].kind, Is.EqualTo("Method"));
        Assert.That(json.items[0].index, Is.EqualTo(2));
        Assert.That(json.items[1].label, Is.EqualTo("lbl"));
        Assert.That(json.items[1].obisCode, Is.EqualTo("0.2.96.3.10.255"));
        Assert.That(json.items[1].kind, Is.EqualTo("Method"));
        Assert.That(json.items[1].index, Is.EqualTo(1));    
    }

    internal class TestOnDemandSetDto
    {
        public TestOnDemandDto[] items { get; set; }
    }

    internal class TestOnDemandDto
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public string kind { get; set; }
        public int index { get; set; }
    }

    [Test]
    public async Task GetOnDemand_QueryParameterLabelAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetOnDemand_QueryParameterLabelEmpty()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private void SetupDisconnectControlCacheGetOutputStatus(params (ISeriesName SeriesName, bool Output)[] values)
    {
        disconnectControlCache.Setup(x => x.GetOutputStatus(It.IsAny<string>()))
            .Returns(values.ToDictionary(x => x.SeriesName, x => x.Output));
    }

}
