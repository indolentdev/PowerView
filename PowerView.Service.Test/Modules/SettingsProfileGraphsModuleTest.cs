using System;
using System.Linq;
using System.Text;
using PowerView.Model;
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
    private ILocationContext locationContext;

    private Browser browser;

    private const string ProfileGraphsRoute = "/api/settings/profilegraphs";
    private const string ProfileGraphsSeriesRoute = ProfileGraphsRoute + "/series";
    private const string ProfileGraphsPagesRoute = ProfileGraphsRoute + "/pages";
    private const string ProfileGraphsSwapRankRoute = ProfileGraphsRoute + "/swaprank";
    private const string ProfileGrahpsModifyRoute = ProfileGraphsRoute + "/modify/{0}";

    [SetUp]
    public void SetUp()
    {
      serieNameRepository= new Mock<ISeriesNameRepository>();
      profileGraphRepository = new Mock<IProfileGraphRepository>();
      locationContext = TimeZoneHelper.GetDenmarkLocationContext();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsProfileGraphsModule>();
        cfg.Dependency<ISeriesNameRepository>(serieNameRepository.Object);
        cfg.Dependency<IProfileGraphRepository>(profileGraphRepository.Object);
        cfg.Dependency<ILocationContext>(locationContext);
      });
    }

    [Test]
    public void GetProfileGraphSeries()
    {
      // Arrange
      const string label = "label";
      var serieNames = new[] { new SeriesName(label, ObisCode.ElectrActiveEnergyA14Delta), new SeriesName(label, ObisCode.ElectrActiveEnergyA14Period), 
        new SeriesName(label, ObisCode.ElectrActualPowerP14), new SeriesName(label, ObisCode.ElectrActiveEnergyA14NetDelta) };
      serieNameRepository.Setup(snr => snr.GetSeriesNames(It.IsAny<TimeZoneInfo>())).Returns(serieNames);

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
      Assert.That(json.items.Length, Is.EqualTo(9));
      AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA14Period, json.items[0]);
      AssertProfileGraphSerie("day", label, ObisCode.ElectrActualPowerP14, json.items[1]);
      AssertProfileGraphSerie("day", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[2]);
      AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[3]);
      AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14Period, json.items[4]);
      AssertProfileGraphSerie("month", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[5]);
      AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Delta, json.items[6]);
      AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14Period, json.items[7]);
      AssertProfileGraphSerie("year", label, ObisCode.ElectrActiveEnergyA14NetDelta, json.items[8]);
      serieNameRepository.Verify(snr => snr.GetSeriesNames(locationContext.TimeZoneInfo));
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
    public void PutProfileGraph()
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
      profileGraphRepository.Setup(x => x.UpdateProfileGraph(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProfileGraph>()))
        .Returns(true);

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, EncodeUpdateProfileGraphIdUrlSegment(oldPeriod, oldPage, oldTitle));
      var response = browser.Put(url, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      profileGraphRepository.Verify(pgr => pgr.UpdateProfileGraph(It.Is<string>(x => x == oldPeriod),
        It.Is<string>(x => x == oldPage), It.Is<string>(x => x == oldTitle), It.Is<ProfileGraph>(pg =>
          pg.Period == profileGraph.Period && pg.Page == profileGraph.Page && pg.Title == profileGraph.Title &&
          pg.Interval == profileGraph.Interval && pg.SerieNames.Count == profileGraph.Series.Length &&
          pg.SerieNames.First().Label == profileGraph.Series[0].Label &&
          pg.SerieNames.First().ObisCode == profileGraph.Series[0].ObisCode)));
    }

    [Test]
    public void PutProfileGraphNoContent()
    {
      // Arrange

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, EncodeUpdateProfileGraphIdUrlSegment("Period", "Page", "Title"));
      var response = browser.Put(url, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public void PutProfileGraphBadBase64Url()
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

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, "NotBase64String");
      var response = browser.Put(url, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public void PutProfileGraphBadJsonUrl()
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

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, Convert.ToBase64String(Encoding.ASCII.GetBytes("NotJsonString")));
      var response = browser.Put(url, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public void PutProfileGraphBadContent()
    {
      // Arrange
      var profileGraph = new { Series = new object[0] };

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, EncodeUpdateProfileGraphIdUrlSegment("Period", "Page", "Title"));
      var response = browser.Put(url, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(profileGraph);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    [Test]
    public void PutProfileGraphDoesNotExist()
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

      // Act
      string url = string.Format(ProfileGrahpsModifyRoute, EncodeUpdateProfileGraphIdUrlSegment(oldPeriod, oldPage, oldTitle));
      var response = browser.Put(url, with =>
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

    private dynamic EncodeUpdateProfileGraphIdUrlSegment(string period, string page, string title)
    {
      var obj = new { period, page, title };
      var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
      return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(json));
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

