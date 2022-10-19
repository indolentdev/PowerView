using System;
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
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

public class GaugeControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IGaugeRepository> gaugeRepository;

    [SetUp]
    public void Setup()
    {
        gaugeRepository = new Mock<IGaugeRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(gaugeRepository.Object);
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
    public async Task GetLatest()
    {
        // Arrange
        var gv = new GaugeValue("Lbl", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour));
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new[] { gv });
        gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/latest");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestGaugeDto>();
        Assert.That(json.timestamp, Is.Not.Null);
        Assert.That(json.groups.Length, Is.EqualTo(1));
        Assert.That(json.groups.First().name, Is.EqualTo("Latest"));
        Assert.That(json.groups.First().registers.Length, Is.EqualTo(1));
        AssertGaugeVaue(gv, json.groups.First().registers.First());
    }

    [Test]
    public async Task GetLatestAbsentTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/latest");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public async Task GetLatestPresentTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });
        var dateTime = new DateTime(2016, 5, 6, 3, 4, 5, DateTimeKind.Utc);

        // Act
        var response = await httpClient.GetAsync($"api/gauges/latest?timestamp={dateTime.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt == dateTime)));
    }

    [Test]
    public async Task GetLatestBadTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/latest?timestamp=badTimestamp");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        gaugeRepository.Verify(gr => gr.GetLatest(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetLatestNotUtcTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/latest?timestamp={DateTime.Now.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        gaugeRepository.Verify(gr => gr.GetLatest(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCustom()
    {
        // Arrange
        var gv = new GaugeValue("Lbl", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour));
        var gvs = new GaugeValueSet(GaugeSetName.Custom, new[] { gv });
        gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/custom");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestGaugeDto>();
        Assert.That(json.timestamp, Is.Not.Null);
        Assert.That(json.groups.Length, Is.EqualTo(1));
        Assert.That(json.groups.First().name, Is.EqualTo("Custom"));
        Assert.That(json.groups.First().registers.Length, Is.EqualTo(1));
        AssertGaugeVaue(gv, json.groups.First().registers.First());
    }

    [Test]
    public async Task GetCustomAbsentTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/custom");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public async Task GetCustomPresentTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });
        var dateTime = new DateTime(2016, 5, 6, 3, 4, 5, DateTimeKind.Utc);

        // Act
        var response = await httpClient.GetAsync($"api/gauges/custom?timestamp={dateTime.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt == dateTime)));
    }

    [Test]
    public async Task GetCustomBadTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/custom?timestamp=badTimestamp");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        gaugeRepository.Verify(gr => gr.GetLatest(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCustomNotUtcTimestamp()
    {
        // Arrange
        var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
        gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

        // Act
        var response = await httpClient.GetAsync($"api/gauges/custom?timestamp={DateTime.Now.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        gaugeRepository.Verify(gr => gr.GetLatest(It.IsAny<DateTime>()), Times.Never);
    }

    private static void AssertGaugeVaue(GaugeValue expected, TestGaugeValueDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.label, Is.EqualTo(expected.Label));
        Assert.That(actual.timestamp, Is.EqualTo(expected.DateTime.ToString("o").Substring(0, 16) + "Z"));
        Assert.That(actual.obiscode, Is.EqualTo(expected.ObisCode.ToString()));
        Assert.That(actual.value, Is.EqualTo(ValueAndUnitConverter.Convert(expected.UnitValue.Value, expected.UnitValue.Unit)));
        Assert.That(actual.unit, Is.EqualTo(ValueAndUnitConverter.Convert(expected.UnitValue.Unit)));
    }

    internal class TestGaugeDto
    {
        public string timestamp { get; set; }
        public TestGaugeValueSetDto[] groups { get; set; }
    }

    internal class TestGaugeValueSetDto
    {
        public string name { get; set; }
        public TestGaugeValueDto[] registers { get; set; }
    }

    internal class TestGaugeValueDto
    {
        public string label { get; set; }
        public string timestamp { get; set; }
        public string obiscode { get; set; }
        public double value { get; set; }
        public string unit { get; set; }
    }

}