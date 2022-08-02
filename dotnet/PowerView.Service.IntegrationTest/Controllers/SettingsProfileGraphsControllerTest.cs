using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Controllers;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;
using PowerView.Service.Mqtt;
using PowerView.Service.Translations;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsProfileGraphsControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ISeriesNameRepository> serieNameRepository;
    private Mock<IProfileGraphRepository> profileGraphRepository;
    private ILocationContext locationContext;

    private const string ProfileGraphsRoute = "/api/settings/profilegraphs";
    private const string ProfileGraphsSeriesRoute = ProfileGraphsRoute + "/series";
    private const string ProfileGraphsPagesRoute = ProfileGraphsRoute + "/pages";
    private const string ProfileGraphsSwapRankRoute = ProfileGraphsRoute + "/swaprank";
    private const string ProfileGrahpsModifyRoute = ProfileGraphsRoute + "/modify/{0}";

    [SetUp]
    public void SetUp()
    {
        serieNameRepository = new Mock<ISeriesNameRepository>();
        profileGraphRepository = new Mock<IProfileGraphRepository>();
        locationContext = TimeZoneHelper.GetDenmarkLocationContext();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(serieNameRepository.Object);
                    sc.AddSingleton(profileGraphRepository.Object);
                    sc.AddSingleton(locationContext);
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
    public async Task GetProfileGraphSeries()
    {
        // Arrange
        const string label = "label";
        var serieNames = new[] { new SeriesName(label, ObisCode.ElectrActiveEnergyA14Delta), new SeriesName(label, ObisCode.ElectrActiveEnergyA14Period),
            new SeriesName(label, ObisCode.ElectrActualPowerP14),
            new SeriesName(label, ObisCode.ElectrActiveEnergyA14NetDelta), new SeriesName(label, ObisCode.ElectrActiveEnergyA23NetDelta) };
        serieNameRepository.Setup(snr => snr.GetSeriesNames(It.IsAny<TimeZoneInfo>())).Returns(serieNames);

        // Act
        var response = await httpClient.GetAsync($"api/settings/profilegraphs/series");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestProfileGraphSerieSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items.Length, Is.EqualTo(12));
        AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA14Period, json.items[0]);
        AssertProfileGraphSerie("day", label, ObisCode.ElectrActualPowerP14, json.items[1]);
        AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[2]);
        AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA23NetDelta, json.items[3]);
        AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[4]);
        AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Period, json.items[5]);
        AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[6]);
        AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA23NetDelta, json.items[7]);
        AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[8]);
        AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Period, json.items[9]);
        AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[10]);
        AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA23NetDelta, json.items[11]);
        serieNameRepository.Verify(snr => snr.GetSeriesNames(locationContext.TimeZoneInfo));
    }

    [Test]
    public async Task GetProfileGraphPages()
    {
        // Arrange
        const string period = "month";
        var pages = new[] { "Page1", "Page2" };
        profileGraphRepository.Setup(pgr => pgr.GetProfileGraphPages(It.IsAny<string>())).Returns(pages);

        // Act
        var response = await httpClient.GetAsync($"api/settings/profilegraphs/pages?period={period}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestProfileGraphPageSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items, Is.EqualTo(pages));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphPages(period));
    }

    [Test]
    public async Task GetProfileGraphPagesQueryAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/settings/profilegraphs/pages");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphPages(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [TestCase("da")]
    [TestCase("monthh")]
    public async Task GetProfileGraphPagesQueryInvalid(string period)
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/settings/profilegraphs/pages?period={period}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphPages(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetProfileGraphs()
    {
        // Arrange
        var profileGraph1 = new ProfileGraph("day", "Page1", "T1", "5-minutes", 1, new[] { new SeriesName("label", "1.2.3.4.5.6") });
        var profileGraph2 = new ProfileGraph("month", "Page2", "T2", "1-days", 2, new[] { new SeriesName("label", "1.2.3.4.5.6") });
        profileGraphRepository.Setup(pgr => pgr.GetProfileGraphs()).Returns(new[] { profileGraph1, profileGraph2 });

        // Act
        var response = await httpClient.GetAsync($"api/settings/profilegraphs");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestProfileGraphSetDto>();
        Assert.That(json.items.Length, Is.EqualTo(2));
        AssertProfileGraph(profileGraph1, json.items[0]);
        AssertProfileGraph(profileGraph2, json.items[1]);
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs());
    }

    [Test]
    public async Task PostProfileGraph()
    {
        // Arrange
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PostAsync($"api/settings/profilegraphs", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        profileGraphRepository.Verify(pgr => pgr.AddProfileGraph(It.Is<ProfileGraph>(pg =>
          pg.Period == profileGraph.Period && pg.Page == profileGraph.Page && pg.Title == profileGraph.Title &&
          pg.Interval == profileGraph.Interval && pg.SerieNames.Count == profileGraph.Series.Length &&
          pg.SerieNames.First().Label == profileGraph.Series[0].Label &&
          pg.SerieNames.First().ObisCode == profileGraph.Series[0].ObisCode)));
    }

    [Test]
    public async Task PostProfileGraphNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/profilegraphs", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostProfileGraphBad()
    {
        // Arrange
        var profileGraph = new { Series = new object[0] };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PostAsync($"api/settings/profilegraphs", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostProfileGraphAlreadyExists()
    {
        // Arrange
        profileGraphRepository.Setup(pgr => pgr.AddProfileGraph(It.IsAny<ProfileGraph>())).Throws(new DataStoreUniqueConstraintException());
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PostAsync($"api/settings/profilegraphs", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PutProfileGraph()
    {
        // Arrange
        const string oldPeriod = "month";
        const string oldPage = "Old Page";
        const string oldTitle = "Old Title";
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);
        profileGraphRepository.Setup(x => x.UpdateProfileGraph(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProfileGraph>()))
          .Returns(true);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period={oldPeriod}&page={oldPage}&title={oldTitle}", content);

        // Assert
        var s = await response.Content.ReadAsStringAsync();
        Assert.That(s, Is.Empty);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        profileGraphRepository.Verify(pgr => pgr.UpdateProfileGraph(It.Is<string>(x => x == oldPeriod),
          It.Is<string>(x => x == oldPage), It.Is<string>(x => x == oldTitle), It.Is<ProfileGraph>(pg =>
            pg.Period == profileGraph.Period && pg.Page == profileGraph.Page && pg.Title == profileGraph.Title &&
            pg.Interval == profileGraph.Interval && pg.SerieNames.Count == profileGraph.Series.Length &&
            pg.SerieNames.First().Label == profileGraph.Series[0].Label &&
            pg.SerieNames.First().ObisCode == profileGraph.Series[0].ObisCode)));
    }

    [Test]
    public async Task PutProfileGraphPeriodQueryParameterAbsent()
    {
        // Arrange
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?page=page&title=title", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutProfileGraphPageQueryParameterAbsent()
    {
        // Arrange
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period=period&title=title", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutProfileGraphTitleQueryParameterAbsent()
    {
        // Arrange
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period=period&page=page", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutProfileGraphNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period=period&page=page&title=title", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutProfileGraphBadContent()
    {
        // Arrange
        var profileGraph = new { Series = new object[0] };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period=period&page=page&title=title", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutProfileGraphDoesNotExist()
    {
        // Arrange
        var profileGraph = new
        {
            Period = "day",
            Page = "The Page",
            Title = "The Title",
            Interval = "5-minutes",
            Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } }
        };
        var content = JsonContent.Create(profileGraph);

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs?period=period&page=page&title=title", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteProfileGraph()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/profilegraphs?period=day&page=ThePage&title=TheTitle");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        profileGraphRepository.Verify(err => err.DeleteProfileGraph("day", "ThePage", "TheTitle"));
    }

    [Test]
    public async Task DeleteProfileGraphPageEmpty()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/profilegraphs?period=day&page=&title=TheTitle");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        profileGraphRepository.Verify(err => err.DeleteProfileGraph("day", "", "TheTitle"));
    }

    [Test]
    public async Task DeleteProfileGraphPeriodQueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/profilegraphs?page=ThePage&title=TheTitle");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.DeleteProfileGraph(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteProfileGraphPageQueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/profilegraphs?period=day&title=TheTitle");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.DeleteProfileGraph(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteProfileGraphTitleQueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/profilegraphs?period=day&page=ThePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.DeleteProfileGraph(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SwapProfileGraphRank()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?period=day&page=ThePage&title1=TheTitle1&title2=TheTitle2", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        profileGraphRepository.Verify(err => err.SwapProfileGraphRank("day", "ThePage", "TheTitle1", "TheTitle2"));
    }

    [Test]
    public async Task SwapProfileGraphRankPeriodQueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?page=ThePage&title1=TheTitle1&title2=TheTitle2", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SwapProfileGraphRankPageQueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?period=day&title1=TheTitle1&title2=TheTitle2", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SwapProfileGraphRankTitle1QueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?period=day&page=ThePage&title2=TheTitle2", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SwapProfileGraphRankTitle2QueryParameterAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?period=day&page=ThePage&title1=TheTitle1", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(err => err.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    [Test]
    public async Task SwapProfileGraphRankConflict()
    {
        // Arrange
        profileGraphRepository.Setup(pgr => pgr.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                              .Throws(new DataStoreUniqueConstraintException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/profilegraphs/swaprank?period=day&page=ThePage&title1=TheTitle1&title2=TheTitle2", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    private void AssertProfileGraphSerie(string period, string label, ObisCode obisCode, TestProfileGraphSerieDto profileGraphSerie)
    {
        Assert.That(profileGraphSerie.period, Is.EqualTo(period));
        Assert.That(profileGraphSerie.label, Is.EqualTo(label));
        Assert.That(profileGraphSerie.obisCode, Is.EqualTo(obisCode.ToString()));
    }

    private void AssertProfileGraph(ProfileGraph profileGraph, ProfileGraphDto dto)
    {
        Assert.That(dto.Period, Is.EqualTo(profileGraph.Period));
        Assert.That(dto.Page, Is.EqualTo(profileGraph.Page));
        Assert.That(dto.Title, Is.EqualTo(profileGraph.Title));
        Assert.That(dto.Interval, Is.EqualTo(profileGraph.Interval));
        Assert.That(dto.Series.Length, Is.EqualTo(profileGraph.SerieNames.Count));
        for (int ix = 0; ix < profileGraph.SerieNames.Count; ix++)
        {
            AssertProfileGraphSerieName(profileGraph.SerieNames[ix], dto.Series[ix]);
        }
    }

    private void AssertProfileGraphSerieName(SeriesName serieName, ProfileGraphSerieDto dto)
    {
        Assert.That(dto.Label, Is.EqualTo(serieName.Label));
        Assert.That(dto.ObisCode, Is.EqualTo(serieName.ObisCode.ToString()));
    }

    internal class TestProfileGraphSerieSetDto
    {
        public TestProfileGraphSerieDto[] items { get; set; }
    }

    internal class TestProfileGraphSerieDto
    {
        public string period { get; set; }
        public string label { get; set; }
        public string obisCode { get; set; }
    }

    internal class TestProfileGraphPageSetDto
    {
        public string[] items { get; set; }
    }

    internal class TestProfileGraphSetDto
    {
        public ProfileGraphDto[] items { get; set; }
    }

}
