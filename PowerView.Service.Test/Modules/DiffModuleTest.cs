using System;
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Model.Expression;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System.Linq;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class DiffModuleTest
  {
    private Mock<IProfileRepository> profileRepository;
    private Mock<ITemplateConfigProvider> templateConfigProvider;
    private Mock<ILocationProvider> locationProvider;

    private Browser browser;

    private const string DiffRoute = "/api/diff";

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();
      locationProvider = new Mock<ILocationProvider>();

      templateConfigProvider.Setup(mcp => mcp.LabelObisCodeTemplates).Returns(new LabelObisCodeTemplate[0]);

      browser = new Browser(cfg =>
      {
        cfg.Module<DiffModule>();
        cfg.Dependency<IProfileRepository>(profileRepository.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
        cfg.Dependency<ILocationProvider>(locationProvider.Object);
      });
    }

    [Test]
    public void GetDiffFromAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("to", today.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffToAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffToBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", "BadFormat");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffFromAndToPresent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var utcOneDay = today.AddDays(1);
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(today, utcOneDay, new LabelSeries<TimeRegisterValue>[0]));
      SetupLocationProvider();

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", utcOneDay.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      profileRepository.Verify(pr => pr.GetMonthProfileSet(It.Is<DateTime>(dt => dt == today.AddHours(-12) && dt.Kind == today.Kind),
                                                           It.Is<DateTime>(dt => dt == today && dt.Kind == today.Kind), 
                                                           It.Is<DateTime>(dt => dt == utcOneDay && dt.Kind == today.Kind)));
    }

    [Test]
    public void Get()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var t1 = today-TimeSpan.FromDays(2);
      var t2 = today-TimeSpan.FromDays(1);
      var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"8.0.1.0.0.255", new [] { new TimeRegisterValue("1", t1, 2, 2, Unit.CubicMetre), new TimeRegisterValue("1", t2, 3, 2, Unit.CubicMetre) } }
      };
      var label2Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } },
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour) } }
      };
      var lss = new LabelSeriesSet<TimeRegisterValue>(t1, today, new[] { new LabelSeries<TimeRegisterValue>("Label1", label1Values), new LabelSeries<TimeRegisterValue>("Label2", label2Values) });
      profileRepository.Setup(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);
      SetupLocationProvider();

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", t1.ToString("o"));
        with.Query("to", today.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<DiffRoot>();
      Assert.That(json.from, Is.EqualTo(t1.ToString("o")));
      Assert.That(json.to, Is.EqualTo(today.ToString("o")));

      Assert.That(json.registers, Has.Length.EqualTo(2));
      AssertDiffRegister("Label1", ObisCode.ColdWaterVolume1Period, t1, t2, 100, "m3", json.registers.First());
      AssertDiffRegister("Label2", ObisCode.ElectrActiveEnergyA14Period, t1, t2, 1000, "kWh", json.registers.Last());
    }

    private TimeZoneInfo SetupLocationProvider()
    {
      var timeZoneInfo = TimeZoneHelper.GetDenmarkTimeZoneInfo();
      locationProvider.Setup(x => x.GetTimeZone()).Returns(timeZoneInfo);
      return timeZoneInfo;
    }

    private static void AssertDiffRegister(string label, ObisCode obisCode, DateTime from, DateTime to, double value, string unit, DiffRegister actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(label));
      Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
      Assert.That(actual.from, Is.EqualTo(from.ToString("o")));
      Assert.That(actual.to, Is.EqualTo(to.ToString("o")));
      Assert.That(actual.value, Is.EqualTo(value));
      Assert.That(actual.unit, Is.EqualTo(unit));
    }

    internal class DiffRoot
    {
      public string from { get; set; }
      public string to { get; set; }
      public DiffRegister[] registers { get; set; }
    }

    internal class DiffRegister
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public string from { get; set; }
      public string to { get; set; }
      public double value { get; set; }
      public string unit { get; set; }
    }

  }
}
