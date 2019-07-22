using System.Collections.Generic;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Expression;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsProfileGraphsModuleTest
  {
    private Mock<ISeriesNameRepository> serieNameRepository;
    private Mock<IProfileGraphRepository> profileGraphRepository;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    private const string ProfileGraphsRoute = "/api/settings/profilegraphs";
    private const string ProfileGraphsSeriesRoute = ProfileGraphsRoute + "/series";
    private const string ProfileGraphsPagesRoute = ProfileGraphsRoute + "/pages";
    private const string ProfileGraphsSwapRankRoute = ProfileGraphsRoute + "/swaprank";

    [SetUp]
    public void SetUp()
    {
      serieNameRepository= new Mock<ISeriesNameRepository>();
      profileGraphRepository = new Mock<IProfileGraphRepository>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsProfileGraphsModule>();
        cfg.Dependency<ISeriesNameRepository>(serieNameRepository.Object);
        cfg.Dependency<IProfileGraphRepository>(profileGraphRepository.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetProfileGraphSeries()
    {
      // Arrange
      const string label = "label";
      var serieNames = new[] { new SeriesName(label, ObisCode.ElectrActiveEnergyA14Delta), new SeriesName(label, ObisCode.ElectrActiveEnergyA14Period), new SeriesName(label, ObisCode.ElectrActualPowerP14) };
      serieNameRepository.Setup(snr => snr.GetSeriesNames(It.IsAny<ICollection<LabelObisCodeTemplate>>())).Returns(serieNames);
      var labelObisCodeTemplates = new LabelObisCodeTemplate[0];
      templateConfigProvider.Setup(tcp => tcp.LabelObisCodeTemplates).Returns(labelObisCodeTemplates);

      // Act
      var response = browser.Get(ProfileGraphsSeriesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestProfileGraphSerieSetDto>();
      Assert.That(json.items, Is.Not.Null);
      Assert.That(json.items.Length, Is.EqualTo(6));
      AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA14Period, json.items[0]);
      AssertProfileGraphSerie("day", label, ObisCode.ElectrActualPowerP14, json.items[1]);
      AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[2]);
      AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Period, json.items[3]);
      AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[4]);
      AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Period, json.items[5]);
      serieNameRepository.Verify(snr => snr.GetSeriesNames(labelObisCodeTemplates));
    }

    [Test]
    public void GetProfileGraphPages()
    {
      // Arrange
      const string period = "month";
      var pages = new[] { "Page1", "Page2" };
      profileGraphRepository.Setup(pgr => pgr.GetProfileGraphPages(It.IsAny<string>())).Returns(pages);

      // Act
      var response = browser.Get(ProfileGraphsPagesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("period", period);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestProfileGraphPageSetDto>();
      Assert.That(json.items, Is.Not.Null);
      Assert.That(json.items, Is.EqualTo(pages));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphPages(period));
    }

    [Test]
    public void GetProfileGraphs()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("day", "Page1", "T1", "5-minutes", 1, new[] { new SeriesName("label", "1.2.3.4.5.6") });
      var profileGraph2 = new ProfileGraph("month", "Page2", "T2", "1-days", 2, new[] { new SeriesName("label", "1.2.3.4.5.6") });
      profileGraphRepository.Setup(pgr => pgr.GetProfileGraphs()).Returns(new[] { profileGraph1, profileGraph2 });

      // Act
      var response = browser.Get(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestProfileGraphSetDto>();
      Assert.That(json.items.Length, Is.EqualTo(2));
      AssertProfileGraph(profileGraph1, json.items[0]);
      AssertProfileGraph(profileGraph2, json.items[1]);
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs());
    }

    [Test]
    public void PostProfileGraph()
    {
      // Arrange
      var profileGraph = new { Period = "day", Page = "The Page", Title = "The Title", Interval="5-minutes",
        Series = new [] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } } };

      // Act
      var response = browser.Post(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(pgr => pgr.AddProfileGraph(It.Is<ProfileGraph>(pg => 
        pg.Period == profileGraph.Period && pg.Page == profileGraph.Page && pg.Title == profileGraph.Title && 
        pg.Interval == profileGraph.Interval && pg.SerieNames.Count == profileGraph.Series.Length &&
        pg.SerieNames.First().Label == profileGraph.Series[0].Label && 
        pg.SerieNames.First().ObisCode == profileGraph.Series[0].ObisCode)));
    }

    [Test]
    public void PostProfileGraphNoContent()
    {
      // Arrange

      // Act
      var response = browser.Post(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }
 
    [Test]
    public void PostProfileGraphBad()
    {
      // Arrange
      var profileGraph = new { Series = new object[0] };

      // Act
      var response = browser.Post(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public void PostProfileGraphAlreadyExists()
    {
      // Arrange
      var profileGraph = new { Period = "day", Page = "The Page", Title = "The Title", Interval = "5-minutes",
        Series = new[] { new { Label = "The Label", ObisCode = "1.2.3.4.5.6" } } };
      profileGraphRepository.Setup(pgr => pgr.AddProfileGraph(It.IsAny<ProfileGraph>())).Throws(new DataStoreUniqueConstraintException());

      // Act
      var response = browser.Post(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public void DeleteProfileGraph()
    {
      // Arrange

      // Act
      var response = browser.Delete(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("period", "day");
        with.Query("page", "ThePage");
        with.Query("title", "TheTitle");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(err => err.DeleteProfileGraph("day", "ThePage", "TheTitle"));
    }

    [Test]
    public void DeleteProfileGraphPageEmpty()
    {
      // Arrange

      // Act
      var response = browser.Delete(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("period", "day");
        with.Query("page", "");
        with.Query("title", "TheTitle");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(err => err.DeleteProfileGraph("day", "", "TheTitle"));
    }

    [Test]
    public void DeleteProfileGraphAbsentParameters()
    {
      // Arrange

      // Act
      var response = browser.Delete(ProfileGraphsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(err => err.DeleteProfileGraph(null, null, null));
    }

    [Test]
    public void SwapProfileGraphRank()
    {
      // Arrange

      // Act
      var response = browser.Put(ProfileGraphsSwapRankRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("period", "day");
        with.Query("page", "ThePage");
        with.Query("title1", "TheTitle1");
        with.Query("title2", "TheTitle2");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(err => err.SwapProfileGraphRank("day", "ThePage", "TheTitle1", "TheTitle2"));
    }

    [Test]
    public void SwapProfileGraphRankAbsentParameters()
    {
      // Arrange

      // Act
      var response = browser.Put(ProfileGraphsSwapRankRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(err => err.SwapProfileGraphRank(null, null, null, null));
    }

    [Test]
    public void SwapProfileGraphRankConflict()
    {
      // Arrange
      profileGraphRepository.Setup(pgr => pgr.SwapProfileGraphRank(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Throws(new DataStoreUniqueConstraintException());

      // Act
      var response = browser.Put(ProfileGraphsSwapRankRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

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
      Assert.That(dto.Rank, Is.EqualTo(profileGraph.Rank));
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
}

