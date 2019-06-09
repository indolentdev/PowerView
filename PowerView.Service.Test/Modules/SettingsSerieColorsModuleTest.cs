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
    private Mock<ISerieColorRepository> serieColorRepository;
    private Mock<ISerieNameRepository> serieNameRepository;
    private Mock<IObisColorProvider> obisColorProvider;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    private const string SerieColorsRoute = "/api/settings/seriecolors";

    [SetUp]
    public void SetUp()
    {
      serieColorRepository = new Mock<ISerieColorRepository>();
      serieNameRepository = new Mock<ISerieNameRepository>();
      obisColorProvider = new Mock<IObisColorProvider>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsSerieColorsModule>();
        cfg.Dependency<ISerieColorRepository>(serieColorRepository.Object);
        cfg.Dependency<ISerieNameRepository>(serieNameRepository.Object);
        cfg.Dependency<IObisColorProvider>(obisColorProvider.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetSerieColors()
    {
      // Arrange
      var sc1 = new SerieColor("label1", "1.2.3.4.5.6", "#123456");
      var sc2 = new SerieColor("label2", "6.5.4.3.2.1", "#654321");
      var serieColorsDb = new[] { sc2, sc1 };
      serieColorRepository.Setup(scr => scr.GetSerieColors()).Returns(serieColorsDb);
      var sn3 = new SerieName("label1", "1.2.3.4.5.6");
      var sn4 = new SerieName("label3", "1.2.3.4.5.6");
      var serieNames = new[] { sn4, sn3 };
      serieNameRepository.Setup(snr => snr.GetSerieNames(It.IsAny<ICollection<LabelObisCodeTemplate>>())).Returns(serieNames);
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
      var json = response.Body.DeserializeJson<TestSerieColorSetDto>();
      Assert.That(json.items, Is.Not.Null);
      Assert.That(json.items.Length, Is.EqualTo(3));
      AssertSerieColor(sc1, json.items.First());
      AssertSerieColor(sc2, json.items.Skip(1).First());
      AssertSerieColor(new SerieColor(sn4.Label, sn4.ObisCode, "#222222"), json.items.Last());
      serieNameRepository.Verify(snr => snr.GetSerieNames(labelObisCodeTemplates));
    }

    private static void AssertSerieColor(SerieColor expected, TestSerieColorDto actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(expected.Label));
      Assert.That(actual.obiscode, Is.EqualTo(expected.ObisCode.ToString()));
      Assert.That(actual.color, Is.EqualTo(expected.Color));
    }

    [Test]
    public void PutSerieColorsEmptyDto()
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
      serieColorRepository.Verify(scr => scr.SetSerieColors(It.IsAny<IEnumerable<SerieColor>>()), Times.Never);
    }

    [Test]
    public void PutSerieColorsEmptyDto2()
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
      serieColorRepository.Verify(scr => scr.SetSerieColors(It.IsAny<IEnumerable<SerieColor>>()), Times.Never);
    }

    [Test]
    public void PutSerieColorsBadColor()
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
      serieColorRepository.Verify(scr => scr.SetSerieColors(It.IsAny<IEnumerable<SerieColor>>()), Times.Never);
    }

    [Test]
    public void PutSerieColors()
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
      serieColorRepository.Verify(scr => scr.SetSerieColors(It.Is<IEnumerable<SerieColor>>(sc => sc.Count() == 1 &&
           sc.First().Label == sc1.label && sc.First().ObisCode == sc1.obisCode && sc.First().Color == sc1.color)));
    }

    private static string ToJson(object obj)
    {
      using (var writer = new StringWriter())
      {
        new JsonSerializer().Serialize(writer, obj);
        return writer.ToString();
      }
    }

    internal class TestSerieColorSetDto
    {
      public TestSerieColorDto[] items { get; set; }
    }

    internal class TestSerieColorDto
    {
      public string label { get; set; }
      public string obiscode { get; set; }
      public string color { get; set; }
      public string serie { get; set; }
    }

  }
}

