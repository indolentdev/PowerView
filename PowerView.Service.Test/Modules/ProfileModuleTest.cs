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
    private Mock<ISerieColorRepository> serieRepository;
    private Mock<IProfileGraphRepository> profileGraphRepository;
    private Mock<ISerieMapper> serieMapper;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      serieRepository = new Mock<ISerieColorRepository>();
      profileGraphRepository = new Mock<IProfileGraphRepository>();
      serieMapper = new Mock<ISerieMapper>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      serieRepository.Setup(sr => sr.GetColorCached(It.IsAny<string>(), It.IsAny<ObisCode>())).Returns<string, ObisCode>((l, oc) => "SC_" + l + "_" + oc);
      serieMapper.Setup(ocm => ocm.MapToSerieType(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "ST_" + oc);
      serieMapper.Setup(ocm => ocm.MapToSerieYAxis(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "YA_" + oc);
      templateConfigProvider.Setup(mcp => mcp.LabelObisCodeTemplates).Returns(new LabelObisCodeTemplate[0]);

      browser = new Browser(cfg =>
      {
        cfg.Module<ProfileModule>();
        cfg.Dependency<IProfileRepository>(profileRepository.Object);
        cfg.Dependency<ISerieColorRepository>(serieRepository.Object);
        cfg.Dependency<IProfileGraphRepository>(profileGraphRepository.Object);
        cfg.Dependency<ISerieMapper>(serieMapper.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetDayProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("day", "ThePage", "title", "5-minutes", 1, new[] { new SerieName("Label", ObisCode.ActiveEnergyA14Interim) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
    }


    [Test]
    public void GetDayProfileStartPassedToRepository()
    {
      // Arrange
      StubProfileGraph();
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

      // Act
      browser.Get("/api/profile/day", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });
        
      // Assert
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind)));
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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDayProfileEnergy()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("day", "thePage", "Import", "5-minutes", 1, new[] { 
        new SerieName("Label1", "6.0.1.0.0.255"), new SerieName("Label1", "6.0.1.0.0.200"), new SerieName("Label1", "6.0.1.0.0.100"), 
        new SerieName("Label1", "6.0.2.0.0.255"), new SerieName("Label1", "6.0.2.0.0.200"), new SerieName("Label1", "6.0.2.0.0.100"), 
        new SerieName("Label1", "6.0.9.0.0.255")
      });
      StubProfileGraph(profileGraph1);
      var now = DateTime.UtcNow;
      var t1 = now-TimeSpan.FromHours(5);
      var t2 = now-TimeSpan.FromHours(4);
      var label1Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"6.0.1.0.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } },
        {"6.0.2.0.0.255", new [] { new TimeRegisterValue("1", t1, 3, 0, Unit.CubicMetre), new TimeRegisterValue("1", t2, 4, 0, Unit.CubicMetre) } },
        {"6.0.9.0.0.255", new [] { new TimeRegisterValue("1", t1, 4, 0, Unit.CubicMetrePrHour), new TimeRegisterValue("1", t2, 5, 0, Unit.CubicMetrePrHour) } }
      };
      var lps = new LabelProfileSet(t1, new [] {new LabelProfile("Label1", t1, label1Values)} );
      profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>())).Returns(lps);

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
      var json = response.Body.DeserializeJson<ViewModelProfileRoot>();
      Assert.That(json.graphs.Length, Is.EqualTo(1));

      var energyImport = new ViewModelProfileGraph { 
        title = profileGraph1.Title,
        categories = new [] { ToStringMinute(t1), ToStringMinute(t2) },
        series = new [] { 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.1.0.0.100", unit = "kWh", serietype = "ST_6.0.1.0.0.100", serieyaxis = "YA_6.0.1.0.0.100", seriecolor = "SC_Label1_6.0.1.0.0.100", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.1.0.0.200", unit = "kWh", serietype = "ST_6.0.1.0.0.200", serieyaxis = "YA_6.0.1.0.0.200", seriecolor = "SC_Label1_6.0.1.0.0.200", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.1.0.0.255", unit = "kWh", serietype = "ST_6.0.1.0.0.255", serieyaxis = "YA_6.0.1.0.0.255", seriecolor = "SC_Label1_6.0.1.0.0.255", values = new double?[] { 2000, 3000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.2.0.0.100", unit = "m3", serietype = "ST_6.0.2.0.0.100", serieyaxis = "YA_6.0.2.0.0.100", seriecolor = "SC_Label1_6.0.2.0.0.100", values = new double?[] { 0, 1 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.2.0.0.200", unit = "m3", serietype = "ST_6.0.2.0.0.200", serieyaxis = "YA_6.0.2.0.0.200", seriecolor = "SC_Label1_6.0.2.0.0.200", values = new double?[] { 0, 1 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.2.0.0.255", unit = "m3", serietype = "ST_6.0.2.0.0.255", serieyaxis = "YA_6.0.2.0.0.255", seriecolor = "SC_Label1_6.0.2.0.0.255", values = new double?[] { 3, 4 } },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.9.0.0.255", unit = "l/h", serietype = "ST_6.0.9.0.0.255", serieyaxis = "YA_6.0.9.0.0.255", seriecolor = "SC_Label1_6.0.9.0.0.255", values = new double?[] { 4000, 5000 } },
        }
      };
      AssertSerieSet(energyImport, json.graphs.First());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "6.0.1.0.0.200", "kWh", 1000d, json.periodtotals[0]);
      AssertPeriodTotals("Label1", "6.0.2.0.0.200", "m3", 1d, json.periodtotals[1]);
    }

    [Test]
    public void GetMonthProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("month", "ThePage", "title", "1-days", 1, new[] { new SerieName("Label", ObisCode.ActiveEnergyA14Interim) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
    }


    [Test]
    public void GetMonthProfileStartPassedToRepository()
    {
      // Arrange
      StubProfileGraph();
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

      // Act
      browser.Get("/api/profile/month", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      profileRepository.Verify(dpr => dpr.GetMonthProfileSet(It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind)));
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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetMonthProfile()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-days", 1, 
        new[] { new SerieName("Label1", "1.0.1.8.0.255"), new SerieName("Label1", "1.0.1.8.0.200"), new SerieName("Label1", "1.0.1.8.0.100") });
      var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-days", 2, 
        new[] { new SerieName("Label0", "1.0.2.8.0.255"), new SerieName("Label0", "1.0.2.8.0.200"), new SerieName("Label0", "1.0.2.8.0.100") });
      StubProfileGraph(profileGraph1, profileGraph2);
      var now = DateTime.UtcNow;
      var t1 = now-TimeSpan.FromDays(5);
      var t2 = now-TimeSpan.FromDays(4);
      var t3 = now-TimeSpan.FromDays(3);
      var label1Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
      var label2Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
      var lps = new LabelProfileSet(t1, new [] {new LabelProfile("Label1", t1, label1Values), new LabelProfile("Label0", t1, label2Values)} );
      profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>())).Returns(lps);

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
        categories = new [] { ToStringDay(t1), ToStringDay(t2) },
        series = new [] { 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.100", unit = "kWh", serietype = "ST_1.0.1.8.0.100", serieyaxis = "YA_1.0.1.8.0.100", seriecolor = "SC_Label1_1.0.1.8.0.100", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.200", unit = "kWh", serietype = "ST_1.0.1.8.0.200", serieyaxis = "YA_1.0.1.8.0.200", seriecolor = "SC_Label1_1.0.1.8.0.200", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.255", unit = "kWh", serietype = "ST_1.0.1.8.0.255", serieyaxis = "YA_1.0.1.8.0.255", seriecolor = "SC_Label1_1.0.1.8.0.255", values = new double?[] { 2000, 3000 } } 
        }
      };
      AssertSerieSet(electricityImport, json.graphs.First());

      var electricityExport = new ViewModelProfileGraph { 
        title = profileGraph2.Title,
        categories = new [] { ToStringDay(t1), ToStringDay(t3) },
        series = new [] { 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.100", unit = "kWh", serietype = "ST_1.0.2.8.0.100", serieyaxis = "YA_1.0.2.8.0.100", seriecolor = "SC_Label0_1.0.2.8.0.100", values = new double?[] { 0, 2000 } }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.200", unit = "kWh", serietype = "ST_1.0.2.8.0.200", serieyaxis = "YA_1.0.2.8.0.200", seriecolor = "SC_Label0_1.0.2.8.0.200", values = new double?[] { 0, 2000 } }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.255", unit = "kWh", serietype = "ST_1.0.2.8.0.255", serieyaxis = "YA_1.0.2.8.0.255", seriecolor = "SC_Label0_1.0.2.8.0.255", values = new double?[] { 4000, 6000 } } 
        }
      };
      AssertSerieSet(electricityExport, json.graphs.Last());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "1.0.1.8.0.200", "kWh", 1000d, json.periodtotals[0]);
      AssertPeriodTotals("Label0", "1.0.2.8.0.200", "kWh", 2000d, json.periodtotals[1]);
    }


    [Test]
    public void GetYearProfilePeriodAndPagePassedToRepository()
    {
      // Arrange
      var profileGraph = new ProfileGraph("year", "ThePage", "title", "1-months", 1, new[] { new SerieName("Label", ObisCode.ActiveEnergyA14Interim) });
      StubProfileGraph(profileGraph);
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

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
      profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetYearProfileStartPassedToRepository()
    {
      // Arrange
      StubProfileGraph();
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

      // Act
      browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", utcNow.ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind)));
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
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>()), Times.Never);
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
      profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetYearProfile()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-months", 1,
        new[] { new SerieName("Label1", "1.0.1.8.0.255"), new SerieName("Label1", "1.0.1.8.0.200"), new SerieName("Label1", "1.0.1.8.0.100") });
      var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-months", 2,
        new[] { new SerieName("Label0", "1.0.2.8.0.255"), new SerieName("Label0", "1.0.2.8.0.200"), new SerieName("Label0", "1.0.2.8.0.100") });
      StubProfileGraph(profileGraph1, profileGraph2);
      var t0 = DateTime.UtcNow - TimeSpan.FromDays(365);
      var t1 = t0.AddMonths(1);
      var t2 = t0.AddMonths(2);
      var t3 = t0.AddMonths(3);
      var label1Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
      var label2Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
      var lps = new LabelProfileSet(t1, new [] {new LabelProfile("Label1", t1, label1Values), new LabelProfile("Label0", t1, label2Values)} );
      profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>())).Returns(lps);

      // Act
      var response = browser.Get("/api/profile/year", with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("start", t0.AddMonths(5).ToString("o"));
        with.Query("page", "thePage");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ViewModelProfileRoot>();
      Assert.That(json.graphs.Length, Is.EqualTo(2));

      var electricityImport = new ViewModelProfileGraph { 
        title = profileGraph1.Title,
        categories = new [] { ToStringMonth(t1), ToStringMonth(t2) },
        series = new [] { 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.100", unit = "kWh", serietype = "ST_1.0.1.8.0.100", serieyaxis = "YA_1.0.1.8.0.100", seriecolor = "SC_Label1_1.0.1.8.0.100", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.200", unit = "kWh", serietype = "ST_1.0.1.8.0.200", serieyaxis = "YA_1.0.1.8.0.200", seriecolor = "SC_Label1_1.0.1.8.0.200", values = new double?[] { 0, 1000 } }, 
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.0.1.8.0.255", unit = "kWh", serietype = "ST_1.0.1.8.0.255", serieyaxis = "YA_1.0.1.8.0.255", seriecolor = "SC_Label1_1.0.1.8.0.255", values = new double?[] { 2000, 3000 } } 
        }
      };
      AssertSerieSet(electricityImport, json.graphs.First());

      var electricityExport = new ViewModelProfileGraph { 
        title = profileGraph2.Title,
        categories = new [] { ToStringMonth(t1), ToStringMonth(t3) },
        series = new [] { 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.100", unit = "kWh", serietype = "ST_1.0.2.8.0.100", serieyaxis = "YA_1.0.2.8.0.100", seriecolor = "SC_Label0_1.0.2.8.0.100", values = new double?[] { 0, 2000 } }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.200", unit = "kWh", serietype = "ST_1.0.2.8.0.200", serieyaxis = "YA_1.0.2.8.0.200", seriecolor = "SC_Label0_1.0.2.8.0.200", values = new double?[] { 0, 2000 } }, 
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.0.2.8.0.255", unit = "kWh", serietype = "ST_1.0.2.8.0.255", serieyaxis = "YA_1.0.2.8.0.255", seriecolor = "SC_Label0_1.0.2.8.0.255", values = new double?[] { 4000, 6000 } } 
        }
      };
      AssertSerieSet(electricityExport, json.graphs.Last());

      Assert.That(json.periodtotals.Length, Is.EqualTo(2));
      AssertPeriodTotals("Label1", "1.0.1.8.0.200", "kWh", 1000d, json.periodtotals[0]);
      AssertPeriodTotals("Label0", "1.0.2.8.0.200", "kWh", 2000d, json.periodtotals[1]);
    }

    private void StubProfileGraph(params ProfileGraph[] profileGraphs)
    {
      var profileGraphsLocal = new List<ProfileGraph>(profileGraphs);

      if (profileGraphsLocal.Count == 0)
      {
        profileGraphsLocal.Add(new ProfileGraph("day", "thePage", "theTitle", "5-minutes", 1, new[] { new SerieName("theLabel", ObisCode.ActiveEnergyA14Interim) }));
      }

      profileGraphRepository.Setup(x => x.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>())).Returns(profileGraphsLocal);
    }

    private static string ToStringMinute(DateTime dt)
    {
      dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0, 0, dt.Kind);
      return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static string ToStringDay(IFormattable dt)
    {
      return dt.ToString("yyyy-MM-ddT12:00Z", CultureInfo.InvariantCulture);
    }

    private static string ToStringMonth(IFormattable dt)
    {
      return dt.ToString("yyyy-MM-01T12:00Z", CultureInfo.InvariantCulture);
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
      public double?[] values { get; set; }
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
