using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PowerView.Model;
using PowerView.Model.Expression;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using Newtonsoft.Json;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsSerieColorsModuleTest
  {
    private Mock<ISeriesColorRepository> seriesColorRepository;
    private Mock<ISeriesNameRepository> seriesNameRepository;
    private Mock<IObisColorProvider> obisColorProvider;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    private const string SerieColorsRoute = "/api/settings/seriecolors";

    [SetUp]
    public void SetUp()
    {
      seriesColorRepository = new Mock<ISeriesColorRepository>();
      seriesNameRepository = new Mock<ISeriesNameRepository>();
      obisColorProvider = new Mock<IObisColorProvider>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsSerieColorsModule>();
        cfg.Dependency<ISeriesColorRepository>(seriesColorRepository.Object);
        cfg.Dependency<ISeriesNameRepository>(seriesNameRepository.Object);
        cfg.Dependency<IObisColorProvider>(obisColorProvider.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetSeriesColors()
    {
      // Arrange
      var sc1 = new SeriesColor(new SeriesName("label1", "1.2.3.4.5.6"), "#123456");
      var sc2 = new SeriesColor(new SeriesName("label2", "6.5.4.3.2.1"), "#654321");
      var serieColorsDb = new[] { sc2, sc1 };
      seriesColorRepository.Setup(scr => scr.GetSeriesColors()).Returns(serieColorsDb);
      var sn3 = new SeriesName("label1", "1.2.3.4.5.6");
      var sn4 = new SeriesName("label3", "1.2.3.4.5.6");
      var serieNames = new[] { sn4, sn3 };
      seriesNameRepository.Setup(snr => snr.GetSeriesNames(It.IsAny<ICollection<LabelObisCodeTemplate>>())).Returns(serieNames);
      obisColorProvider.Setup(ocp => ocp.GetColor(It.IsAny<ObisCode>())).Returns("#222222");
      var labelObisCodeTemplates = new LabelObisCodeTemplate[0];
      templateConfigProvider.Setup(tcp => tcp.LabelObisCodeTemplates).Returns(labelObisCodeTemplates);

      // Act
      var response = browser.Get(SerieColorsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestSeriesColorSetDto>();
      Assert.That(json.items, Is.Not.Null);
      Assert.That(json.items.Length, Is.EqualTo(3));
      AssertSeriesColor(sc1, json.items.First());
      AssertSeriesColor(sc2, json.items.Skip(1).First());
      AssertSeriesColor(new SeriesColor(new SeriesName(sn4.Label, sn4.ObisCode), "#222222"), json.items.Last());
      seriesNameRepository.Verify(snr => snr.GetSeriesNames(labelObisCodeTemplates));
    }

    private static void AssertSeriesColor(SeriesColor expected, TestSeriesColorDto actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(expected.SeriesName.Label));
      Assert.That(actual.obiscode, Is.EqualTo(expected.SeriesName.ObisCode.ToString()));
      Assert.That(actual.color, Is.EqualTo(expected.Color));
    }

    [Test]
    public void PutSeriesColorsEmptyDto()
    {
      // Arrange
      var body = new { };

      // Act
      var response = browser.Put(SerieColorsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(Encoding.UTF8.GetBytes(ToJson(body))), "application/json");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.IsAny<IEnumerable<SeriesColor>>()), Times.Never);
    }

    [Test]
    public void PutSeriesColorsEmptyDto2()
    {
      // Arrange
      var body = new { otherProperty = "unexpectedProperty" };

      // Act
      var response = browser.Put(SerieColorsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(Encoding.UTF8.GetBytes(ToJson(body))), "application/json");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.IsAny<IEnumerable<SeriesColor>>()), Times.Never);
    }

    [Test]
    public void PutSeriesColorsBadColor()
    {
      // Arrange
      var sc1 = new { label = "TheLabel", obisCode = "1.2.3.4.5.6", color = "BadColor", serie = "TheSerie" };
      var body = new { items = new[] { sc1 } };

      // Act
      var response = browser.Put(SerieColorsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(Encoding.UTF8.GetBytes(ToJson(body))), "application/json");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.IsAny<IEnumerable<SeriesColor>>()), Times.Never);
    }

    [Test]
    public void PutSeriesColors()
    {
      // Arrange
      var sc1 = new { label = "TheLabel", obisCode = "1.2.3.4.5.6", color = "#123456", serie = "TheSerie" };
      var body = new { items = new[] { sc1 } };

      // Act
      var response = browser.Put(SerieColorsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(Encoding.UTF8.GetBytes(ToJson(body))), "application/json");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      seriesColorRepository.Verify(scr => scr.SetSeriesColors(It.Is<IEnumerable<SeriesColor>>(sc => sc.Count() == 1 &&
           sc.First().SeriesName.Label == sc1.label && sc.First().SeriesName.ObisCode == sc1.obisCode && sc.First().Color == sc1.color)));
    }

    private static string ToJson(object obj)
    {
      using (var writer = new StringWriter())
      {
        new JsonSerializer().Serialize(writer, obj);
        return writer.ToString();
      }
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
}

