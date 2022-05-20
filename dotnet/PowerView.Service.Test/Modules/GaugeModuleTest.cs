/*
using System;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System.Linq;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class GaugeModuleTest
  {
    private Mock<IGaugeRepository> gaugeRepository;

    private Browser browser;

    private const string GaugesRoute = "/api/gauges";
    private const string GaugesLatestRoute = GaugesRoute + "/latest";
    private const string GaugesCustomRoute = GaugesRoute + "/custom";

    [SetUp]
    public void SetUp()
    {
      gaugeRepository = new Mock<IGaugeRepository>();

      browser = new Browser(cfg =>
      {
        cfg.Module<GaugeModule>();
        cfg.Dependency<IGaugeRepository>(gaugeRepository.Object);
      });
    }

    [Test]
    public void GetLatest()
    {
      // Arrange
      var gv = new GaugeValue("Lbl", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour));
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new[] { gv });
      gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesLatestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestGaugeDto>();
      Assert.That(json.timestamp, Is.Not.Null);
      Assert.That(json.groups.Length, Is.EqualTo(1));
      Assert.That(json.groups.First().name, Is.EqualTo("Latest"));
      Assert.That(json.groups.First().registers.Length, Is.EqualTo(1));
      AssertGaugeVaue(gv, json.groups.First().registers.First());
    }

    [Test]
    public void GetLatestAbsentTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesLatestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public void GetLatestPresentTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });
      var dateTime = new DateTime(2016, 5, 6, 3, 4, 5, DateTimeKind.Utc);

      // Act
      var response = browser.Get(GaugesLatestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", dateTime.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt == dateTime)));
    }

    [Test]
    public void GetLatestBadTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesLatestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", "Bad Timestamp");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public void GetLatestNotUtcTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetLatest(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesLatestRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", DateTime.Now.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetLatest(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public void GetCustom()
    {
      // Arrange
      var gv = new GaugeValue("Lbl", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour));
      var gvs = new GaugeValueSet(GaugeSetName.Custom, new[] { gv });
      gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesCustomRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestGaugeDto>();
      Assert.That(json.timestamp, Is.Not.Null);
      Assert.That(json.groups.Length, Is.EqualTo(1));
      Assert.That(json.groups.First().name, Is.EqualTo("Custom"));
      Assert.That(json.groups.First().registers.Length, Is.EqualTo(1));
      AssertGaugeVaue(gv, json.groups.First().registers.First());
    }

    [Test]
    public void GetCustomAbsentTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesCustomRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public void GetCustomPresentTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });
      var dateTime = new DateTime(2016, 5, 6, 3, 4, 5, DateTimeKind.Utc);

      // Act
      var response = browser.Get(GaugesCustomRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", dateTime.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt == dateTime)));
    }

    [Test]
    public void GetCustomBadTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesCustomRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", "Bad Timestamp");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    [Test]
    public void GetCustomNotUtcTimestamp()
    {
      // Arrange
      var gvs = new GaugeValueSet(GaugeSetName.Latest, new GaugeValue[0]);
      gaugeRepository.Setup(gr => gr.GetCustom(It.IsAny<DateTime>())).Returns(new[] { gvs });

      // Act
      var response = browser.Get(GaugesCustomRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("timestamp", DateTime.Now.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      gaugeRepository.Verify(gr => gr.GetCustom(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc && DateTime.UtcNow - dt < TimeSpan.FromMinutes(1))));
    }

    private static void AssertGaugeVaue(GaugeValue expected, TestGaugeValueDto actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(expected.Label));
      Assert.That(actual.timestamp, Is.EqualTo(expected.DateTime.ToString("o").Substring(0,16) + "Z"));
      Assert.That(actual.obiscode, Is.EqualTo(expected.ObisCode.ToString()));
      Assert.That(actual.value, Is.EqualTo(ValueAndUnitMapper.Map(expected.UnitValue.Value, expected.UnitValue.Unit)));
      Assert.That(actual.unit, Is.EqualTo(ValueAndUnitMapper.Map(expected.UnitValue.Unit)));
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
}
*/