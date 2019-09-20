using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Expression;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using PowerView.Service.Mappers;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class ProfileModuleTest
  {
    private Mock<IProfileRepository> profileRepository;
    private Mock<ISeriesColorRepository> serieRepository;
    private Mock<IProfileGraphRepository> profileGraphRepository;
    private Mock<ILocationProvider> locationProvider;
    private Mock<ISerieMapper> serieMapper;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      serieRepository = new Mock<ISeriesColorRepository>();
      profileGraphRepository = new Mock<IProfileGraphRepository>();
      locationProvider = new Mock<ILocationProvider>();
      serieMapper = new Mock<ISerieMapper>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      serieRepository.Setup(sr => sr.GetColorCached(It.IsAny<string>(), It.IsAny<ObisCode>())).Returns<string, ObisCode>((l, oc) => "SC_" + l + "_" + oc);
      locationProvider.Setup(lp => lp.GetTimeZone()).Returns(TimeZoneInfo.Utc); 
      serieMapper.Setup(ocm => ocm.MapToSerieType(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "ST_" + oc);
      serieMapper.Setup(ocm => ocm.MapToSerieYAxis(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "YA_" + oc);
      templateConfigProvider.Setup(mcp => mcp.LabelObisCodeTemplates).Returns(new LabelObisCodeTemplate[0]);

      browser = new Browser(cfg =>
      {
        cfg.Module<ProfileModule>();
        cfg.Dependency<IProfileRepository>(profileRepository.Object);
        cfg.Dependency<ISeriesColorRepository>(serieRepository.Object);
        cfg.Dependency<IProfileGraphRepository>(profileGraphRepository.Object);
        cfg.Dependency<ILocationProvider>(locationProvider.Object);
        cfg.Dependency<ISerieMapper>(serieMapper.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetDayProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("day", "ThePage", "title", "5-minutes", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddDays(1), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/day", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", profileGraph.Page);
      });

      // Assert
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public void GetDayProfilePageQueryStringAbsent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get("/api/profile/day", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }


    [Test]
    public void GetDayProfileDateTimesPassedToRepository()
    {
      // Arrange
      StubProfileGraph();
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddDays(1), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/day", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });
        
      // Assert
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.Is<DateTime>(dt => dt == utcNow.AddMinutes(-2.5) && dt.Kind == utcNow.Kind),
        It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind), It.Is<DateTime>(dt => dt == utcNow.AddDays(1) && dt.Kind == utcNow.Kind)));
    }

    [Test]
    public void GetDayProfileStartQueryStringAbsent()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/day", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDayProfileStartQueryStringBad()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/day", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", "BadFormat");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDayProfile()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("day", "thePage", "Import", "5-minutes", 1, new[] { 
        new SeriesName("Label1", "6.0.1.0.0.255"), new SeriesName("Label1", "6.66.1.0.0.255"), new SeriesName("Label1", "6.67.8.0.0.255"), 
        new SeriesName("Label1", "6.0.2.0.0.255"), new SeriesName("Label1", "6.66.2.0.0.255"), new SeriesName("Label1", "6.67.9.0.0.255"),
        new SeriesName("Label1", "6.0.9.0.0.255")
      });
      StubProfileGraph(profileGraph1);
      var now = new DateTime(2019,8,30,19,00, 0, DateTimeKind.Utc);
      var t0 = now-TimeSpan.FromMinutes(5);
      var t1 = now;
      var t2 = now+TimeSpan.FromMinutes(5);
      var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"6.0.1.0.0.255", new [] { new TimeRegisterValue("1", t0, 1, 6, Unit.WattHour), new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } },
        {"6.0.2.0.0.255", new [] { new TimeRegisterValue("1", t0, 2, 0, Unit.CubicMetre), new TimeRegisterValue("1", t1, 3, 0, Unit.CubicMetre), new TimeRegisterValue("1", t2, 4, 0, Unit.CubicMetre) } },
        {"6.0.9.0.0.255", new [] { new TimeRegisterValue("1", t1, 4, 0, Unit.CubicMetrePrHour), new TimeRegisterValue("1", t2, 5, 0, Unit.CubicMetrePrHour) } }
      };
      var lss = new LabelSeriesSet<TimeRegisterValue>(now, now.AddDays(1), new [] {new LabelSeries<TimeRegisterValue>("Label1", label1Values)} );
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

      // Act
      var response = browser.Get("/api/profile/day", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", now.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
//      var resp = response.Body.AsString();
      var json = response.Body.DeserializeJson<ViewModelProfileRoot>();
      Assert.That(json.graphs.Length, Is.EqualTo(1));

      var energyImport = new ViewModelProfileGraph {
        title = profileGraph1.Title,
        categories = Enumerable.Range(0, 288).Select(i => now.AddMinutes(i * 5)).Select(ToStringMinute).ToArray(),
        series = new [] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.1.0.0.255", unit = "kWh", serietype = "ST_6.0.1.0.0.255", serieyaxis = "YA_6.0.1.0.0.255", seriecolor = "SC_Label1_6.0.1.0.0.255", 
            values = new object[] { 2000, 3000 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.2.0.0.255", unit = "m3", serietype = "ST_6.0.2.0.0.255", serieyaxis = "YA_6.0.2.0.0.255", seriecolor = "SC_Label1_6.0.2.0.0.255", 
            values = new object[] { 3, 4 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.9.0.0.255", unit = "l/h", serietype = "ST_6.0.9.0.0.255", serieyaxis = "YA_6.0.9.0.0.255", seriecolor = "SC_Label1_6.0.9.0.0.255", 
            values = new object[] { 4000, 5000 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.66.1.0.0.255", unit = "kWh", serietype = "ST_6.66.1.0.0.255", serieyaxis = "YA_6.66.1.0.0.255", seriecolor = "SC_Label1_6.66.1.0.0.255", 
            values = new object[] { 1000, 2000 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.66.2.0.0.255", unit = "m3", serietype = "ST_6.66.2.0.0.255", serieyaxis = "YA_6.66.2.0.0.255", seriecolor = "SC_Label1_6.66.2.0.0.255", 
            values = new object[] { 1, 2 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.67.8.0.0.255", unit = "W", serietype = "ST_6.67.8.0.0.255", serieyaxis = "YA_6.67.8.0.0.255", seriecolor = "SC_Label1_6.67.8.0.0.255", 
            values = new object[] { 12000000, 12000000 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.67.9.0.0.255", unit = "l/h", serietype = "ST_6.67.9.0.0.255", serieyaxis = "YA_6.67.9.0.0.255", seriecolor = "SC_Label1_6.67.9.0.0.255", 
            values = new object[] { 12000, 12000 }.Concat(Enumerable.Repeat<object>(null, 286)).ToArray() },
        }
      };
      AssertSerieSet(energyImport, json.graphs.First());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "6.66.1.0.0.255", "kWh", 2000d, json.periodtotals[0]);
      AssertPeriodTotals("Label1", "6.66.2.0.0.255", "m3", 2d, json.periodtotals[1]);
    }

    [Test]
    public void GetMonthProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("month", "ThePage", "title", "1-days", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddDays(1), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/month", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", profileGraph.Page);
      });

      // Assert
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public void GetMonthProfilePageQueryStringAbsent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get("/api/profile/month", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }


    [Test]
    public void GetMonthProfileDateTimesPassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("month", "ThePage", "title", "1-days", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddDays(1), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/month", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      profileRepository.Verify(dpr => dpr.GetMonthProfileSet(It.Is<DateTime>(dt => dt == utcNow.AddDays(-0.5) && dt.Kind == utcNow.Kind),
        It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind), It.Is<DateTime>(dt => dt == utcNow.AddMonths(1) && dt.Kind == utcNow.Kind)));
    }

    [Test]
    public void GetMonthProfileStartQueryStringAbsent()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/month", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetMonthProfileStartQueryStringBad()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/month", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", "BadFormat");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetMonthProfile()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-days", 1, 
        new[] { new SeriesName("Label1", "1.0.1.8.0.255"), new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
      var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-days", 2, 
        new[] { new SeriesName("Label0", "1.0.2.8.0.255"), new SeriesName("Label0", "1.66.2.8.0.255"), new SeriesName("Label0", "1.65.2.8.0.255") });
      StubProfileGraph(profileGraph1, profileGraph2);
      var now = new DateTime(2019, 3, 1, 0, 0, 0, DateTimeKind.Utc);
      var t1 = now;
      var t2 = now.AddDays(1);
      var t3 = now.AddDays(2);
      var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
      var label0Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
      var lss = new LabelSeriesSet<TimeRegisterValue>(now, now.AddMonths(1), new [] {new LabelSeries<TimeRegisterValue>("Label1", label1Values), new LabelSeries<TimeRegisterValue>("Label0", label0Values)} );
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

      // Act
      var response = browser.Get("/api/profile/month", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", now.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ViewModelProfileRoot>();
      Assert.That(json.graphs.Length, Is.EqualTo(2));

      var electricityImport= new ViewModelProfileGraph { 
        title = profileGraph1.Title,
        categories = Enumerable.Range(0, 31).Select(i => now.AddDays(i)).Select(ToStringDay).ToArray(),
        series = new [] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.255", unit = "kWh", serietype = "ST_1.0.1.8.0.255", serieyaxis = "YA_1.0.1.8.0.255", seriecolor = "SC_Label1_1.0.1.8.0.255", 
            values = new object[] { 2000, 3000 }.Concat(Enumerable.Repeat<object>(null, 29)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.65.1.8.0.255", unit = "kWh", serietype = "ST_1.65.1.8.0.255", serieyaxis = "YA_1.65.1.8.0.255", seriecolor = "SC_Label1_1.65.1.8.0.255", 
            values = new object[] { 0, 1000 }.Concat(Enumerable.Repeat<object>(null, 29)).ToArray() }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.66.1.8.0.255", unit = "kWh", serietype = "ST_1.66.1.8.0.255", serieyaxis = "YA_1.66.1.8.0.255", seriecolor = "SC_Label1_1.66.1.8.0.255", 
            values = new object[] { 0, 1000 }.Concat(Enumerable.Repeat<object>(null, 29)).ToArray() } 
        }
      };
      AssertSerieSet(electricityImport, json.graphs.First());

      var electricityExport = new ViewModelProfileGraph { 
        title = profileGraph2.Title,
        categories = Enumerable.Range(0, 31).Select(i => now.AddDays(i)).Select(ToStringDay).ToArray(),
        series = new [] {
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.255", unit = "kWh", serietype = "ST_1.0.2.8.0.255", serieyaxis = "YA_1.0.2.8.0.255", seriecolor = "SC_Label0_1.0.2.8.0.255", 
            values = new object[] { 4000, null, 6000 }.Concat(Enumerable.Repeat<object>(null, 28)).ToArray() },
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.65.2.8.0.255", unit = "kWh", serietype = "ST_1.65.2.8.0.255", serieyaxis = "YA_1.65.2.8.0.255", seriecolor = "SC_Label0_1.65.2.8.0.255", 
            values = new object[] { 0, null, 2000 }.Concat(Enumerable.Repeat<object>(null, 28)).ToArray() }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.66.2.8.0.255", unit = "kWh", serietype = "ST_1.66.2.8.0.255", serieyaxis = "YA_1.66.2.8.0.255", seriecolor = "SC_Label0_1.66.2.8.0.255", 
            values = new object[] { 0, null, 2000 }.Concat(Enumerable.Repeat<object>(null, 28)).ToArray() } 
        }
      };
      AssertSerieSet(electricityExport, json.graphs.Last());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "1.66.1.8.0.255", "kWh", 1000d, json.periodtotals[0]);
      AssertPeriodTotals("Label0", "1.66.2.8.0.255", "kWh", 2000d, json.periodtotals[1]);
    }


    [Test]
    public void GetYearProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("year", "ThePage", "title", "1-months", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddDays(1), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/year", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", profileGraph.Page);
      });

      // Assert
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public void GetYearProfilePageQueryStringAbsent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get("/api/profile/year", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetYearProfileDateTimesPassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("month", "thePage", "Import", "1-months", 1,
        new[] { new SeriesName("Label1", "1.0.1.8.0.255"), new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
      StubProfileGraph(profileGraph);
      var utcNow = new DateTime(2019, 06, 15, 0, 0, 0, DateTimeKind.Utc);
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(utcNow, utcNow.AddMonths(12), new LabelSeries<TimeRegisterValue>[0]));

      // Act
      browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.Is<DateTime>(dt => dt == utcNow.AddDays(-15) && dt.Kind == utcNow.Kind),
        It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind), It.Is<DateTime>(dt => dt == utcNow.AddMonths(12) && dt.Kind == utcNow.Kind)));
    }

    [Test]
    public void GetYearProfileStartQueryStringAbsent()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetYearProfileStartQueryStringBad()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", "BadFormat");
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetYearProfile()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-months", 1,
        new[] { new SeriesName("Label1", "1.0.1.8.0.255"), new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
      var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-months", 2,
        new[] { new SeriesName("Label0", "1.0.2.8.0.255"), new SeriesName("Label0", "1.66.2.8.0.255"), new SeriesName("Label0", "1.65.2.8.0.255") });
      StubProfileGraph(profileGraph1, profileGraph2);
      var now = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      var t1 = now;
      var t2 = now.AddMonths(1);
      var t3 = now.AddMonths(2);
      var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
      var label2Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
      var lss = new LabelSeriesSet<TimeRegisterValue>(t1, t1.AddMonths(12), new [] {new LabelSeries<TimeRegisterValue>("Label1", label1Values), new LabelSeries<TimeRegisterValue>("Label0", label2Values)} );
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

      // Act
      var response = browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", now.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ViewModelProfileRoot>();
      Assert.That(json.graphs.Length, Is.EqualTo(2));

      var electricityImport = new ViewModelProfileGraph { 
        title = profileGraph1.Title,
        categories = Enumerable.Range(0, 12).Select(i => now.AddMonths(i)).Select(ToStringMonth).ToArray(),
        series = new [] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.255", unit = "kWh", serietype = "ST_1.0.1.8.0.255", serieyaxis = "YA_1.0.1.8.0.255", seriecolor = "SC_Label1_1.0.1.8.0.255", 
            values = new object[] { 2000, 3000 }.Concat(Enumerable.Repeat<object>(null, 10)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.65.1.8.0.255", unit = "kWh", serietype = "ST_1.65.1.8.0.255", serieyaxis = "YA_1.65.1.8.0.255", seriecolor = "SC_Label1_1.65.1.8.0.255", 
            values = new object[] { 0, 1000 }.Concat(Enumerable.Repeat<object>(null, 10)).ToArray() }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.66.1.8.0.255", unit = "kWh", serietype = "ST_1.66.1.8.0.255", serieyaxis = "YA_1.66.1.8.0.255", seriecolor = "SC_Label1_1.66.1.8.0.255", 
            values = new object[] { 0, 1000 }.Concat(Enumerable.Repeat<object>(null, 10)).ToArray() } 
        }
      };
      AssertSerieSet(electricityImport, json.graphs.First());

      var electricityExport = new ViewModelProfileGraph { 
        title = profileGraph2.Title,
        categories = Enumerable.Range(0, 12).Select(i => now.AddMonths(i)).Select(ToStringMonth).ToArray(),
        series = new [] {
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.255", unit = "kWh", serietype = "ST_1.0.2.8.0.255", serieyaxis = "YA_1.0.2.8.0.255", seriecolor = "SC_Label0_1.0.2.8.0.255", 
            values = new object[] { 4000, null, 6000 }.Concat(Enumerable.Repeat<object>(null, 9)).ToArray() },
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.65.2.8.0.255", unit = "kWh", serietype = "ST_1.65.2.8.0.255", serieyaxis = "YA_1.65.2.8.0.255", seriecolor = "SC_Label0_1.65.2.8.0.255", 
            values = new object[] { 0, null, 2000 }.Concat(Enumerable.Repeat<object>(null, 9)).ToArray() }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.66.2.8.0.255", unit = "kWh", serietype = "ST_1.66.2.8.0.255", serieyaxis = "YA_1.66.2.8.0.255", seriecolor = "SC_Label0_1.66.2.8.0.255", 
            values = new object[] { 0, null, 2000 }.Concat(Enumerable.Repeat<object>(null, 9)).ToArray() } 
        }
      };
      AssertSerieSet(electricityExport, json.graphs.Last());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "1.66.1.8.0.255", "kWh", 1000d, json.periodtotals[0]);
      AssertPeriodTotals("Label0", "1.66.2.8.0.255", "kWh", 2000d, json.periodtotals[1]);
    }

    private void StubProfileGraph(params ProfileGraph[] profileGraphs)
    {
      var profileGraphsLocal = new List<ProfileGraph>(profileGraphs);

      if (profileGraphsLocal.Count == 0)
      {
        profileGraphsLocal.Add(new ProfileGraph("day", "thePage", "theTitle", "5-minutes", 1, new[] { new SeriesName("theLabel", ObisCode.ElectrActiveEnergyA14Period) }));
      }

      profileGraphRepository.Setup(x => x.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>())).Returns(profileGraphsLocal);
    }

    private static string ToStringMinute(DateTime dt)
    {
      dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0, 0, dt.Kind);
      return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static string ToStringDay(DateTime dt)
    {
      return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static string ToStringMonth(DateTime dt)
    {
      return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static void AssertSerieSet(ViewModelProfileGraph expected, ViewModelProfileGraph actual)
    {
      Assert.That(actual.title, Is.EqualTo(expected.title));
      Assert.That(actual.categories, Is.EqualTo(expected.categories));
      Assert.That(actual.series.Length, Is.EqualTo(expected.series.Length));
      for (var ix = 0; ix < expected.series.Length; ix++)
      {
        AssertSerie(expected.series[ix], actual.series[ix]);
      }
    }

    private static void AssertSerie(ViewModelProfileSerie expected, ViewModelProfileSerie actual)
    {
      Assert.That(actual.label, Is.EqualTo(expected.label));
      Assert.That(actual.obisCode, Is.EqualTo(expected.obisCode));
      Assert.That(actual.unit, Is.EqualTo(expected.unit), actual.obisCode);
      Assert.That(actual.serietype, Is.EqualTo(expected.serietype), actual.obisCode);
      Assert.That(actual.serieyaxis, Is.EqualTo(expected.serieyaxis), actual.obisCode);
      Assert.That(actual.seriecolor, Is.EqualTo(expected.seriecolor), actual.obisCode);
      Assert.That(actual.values, Is.EqualTo(expected.values), actual.obisCode);
    }

    private static void AssertPeriodTotals(string label, string register, string unit, double value, ViewModelProfilePeriodTotal actual)
    {
      Assert.That(actual.label, Is.EqualTo(label));
      Assert.That(actual.obisCode, Is.EqualTo(register));
      Assert.That(actual.unit, Is.EqualTo(unit));
      Assert.That(actual.value, Is.EqualTo(value));
    }

    public static Stream EqualTo(IEnumerable<byte> expectedBytes)
    {
      return Match.Create<Stream>(s =>
      {
        using (var ms = new MemoryStream())
        {
          s.CopyTo(ms);
          var sArray = ms.ToArray();
          return sArray.SequenceEqual(expectedBytes);
        }
      });
    }

    internal class ViewModelProfileRoot
    {
      public ViewModelProfileGraph[] graphs { get; set; }
      public ViewModelProfilePeriodTotal[] periodtotals { get; set; }
    }

    internal class ViewModelProfileGraph
    {
      public string title { get; set; }
      public string[] categories { get; set; }
      public ViewModelProfileSerie[] series { get; set; }
    }

    internal class ViewModelProfileSerie
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public string unit { get; set; }
      public string serietype { get; set; }
      public string serieyaxis { get; set; }
      public string seriecolor { get; set; }
      public object[] values { get; set; }
    }

    internal class ViewModelProfilePeriodTotal 
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public double value { get; set; }
      public string unit { get; set; }
    }

  }
}
