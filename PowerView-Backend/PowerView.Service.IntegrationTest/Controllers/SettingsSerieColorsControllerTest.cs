using System;
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

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsSerieColorsModuleTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ISeriesColorRepository> seriesColorRepository;
    private Mock<ISeriesNameProvider> seriesNameProvider;
    private Mock<IObisColorProvider> obisColorProvider;

    private const string SerieColorsRoute = "/api/settings/seriecolors";

    [SetUp]
    public void SetUp()
    {
        seriesColorRepository = new Mock<ISeriesColorRepository>();
        seriesNameProvider = new Mock<ISeriesNameProvider>();
        obisColorProvider = new Mock<IObisColorProvider>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(seriesColorRepository.Object);
                    sc.AddSingleton(seriesNameProvider.Object);
                    sc.AddSingleton(obisColorProvider.Object);
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
    public async Task GetSeriesColors()
    {
        // Arrange
        var sc1 = new SeriesColor(new SeriesName("label1", "1.2.3.4.5.6"), "#123456");
        var sc2 = new SeriesColor(new SeriesName("label2", "6.5.4.3.2.1"), "#654321");
        var serieColorsDb = new[] { sc2, sc1 };
        seriesColorRepository.Setup(scr => scr.GetSeriesColors()).Returns(serieColorsDb);
        var sn3 = new SeriesName("label1", "1.2.3.4.5.6");
        var sn4 = new SeriesName("label3", "1.2.3.4.5.6");
        var serieNames = new[] { sn4, sn3 };
        seriesNameProvider.Setup(snr => snr.GetSeriesNames()).Returns(serieNames);
        obisColorProvider.Setup(ocp => ocp.GetColor(It.IsAny<ObisCode>())).Returns("#222222");

        // Act
        var response = await httpClient.GetAsync($"api/settings/seriecolors");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestSeriesColorSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items.Length, Is.EqualTo(3));
        AssertSeriesColor(sc1, json.items.First());
        AssertSeriesColor(sc2, json.items.Skip(1).First());
        AssertSeriesColor(new SeriesColor(new SeriesName(sn4.Label, sn4.ObisCode), "#222222"), json.items.Last());
        seriesNameProvider.Verify(snr => snr.GetSeriesNames());
    }

    private static void AssertSeriesColor(SeriesColor expected, TestSeriesColorDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.label, Is.EqualTo(expected.SeriesName.Label));
        Assert.That(actual.obiscode, Is.EqualTo(expected.SeriesName.ObisCode.ToString()));
        Assert.That(actual.color, Is.EqualTo(expected.Color));
    }

    [Test]
    public async Task PutSeriesColorsEmptyDto()
    {
        // Arrange
        var content = JsonContent.Create(new object());

        // Act
        var response = await httpClient.PutAsync($"api/settings/seriecolors", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.IsAny<IEnumerable<SeriesColor>>()), Times.Never);
    }

    [Test]
    public async Task PutSeriesColorsBadColor()
    {
        // Arrange
        var sc1 = new { label = "TheLabel", obisCode = "1.2.3.4.5.6", color = "BadColor" };
        var content = JsonContent.Create(new { items = new[] { sc1 } });

        // Act
        var response = await httpClient.PutAsync($"api/settings/seriecolors", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.IsAny<IEnumerable<SeriesColor>>()), Times.Never);
    }

    [Test]
    public async Task PutSeriesColors()
    {
        // Arrange
        var sc1 = new { label = "TheLabel", obisCode = "1.2.3.4.5.6", color = "#123456" };
        var content = JsonContent.Create(new { items = new[] { sc1 } });

        // Act
        var response = await httpClient.PutAsync($"api/settings/seriecolors", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.Is<IEnumerable<SeriesColor>>(sc => sc.Count() == 1 &&
             sc.First().SeriesName.Label == sc1.label && sc.First().SeriesName.ObisCode == sc1.obisCode && sc.First().Color == sc1.color)));
    }

    internal class TestSeriesColorSetDto
    {
        public TestSeriesColorDto[] items { get; set; }
    }

    internal class TestSeriesColorDto
    {
        public string label { get; set; }
        public string obiscode { get; set; }
        public string color { get; set; }
        public string serie { get; set; }
    }

}
