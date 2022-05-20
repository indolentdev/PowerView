/*
using System;
using System.Collections.Generic;
using System.Globalization;
using PowerView.Model;
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
  public class ExportModuleTest
  {
    private Mock<ISeriesNameRepository> seriesNameRepository;
    private Mock<IExportRepository> exportRepository;

    private Browser browser;

    private const string ExportLabelsRoute = "/api/export/labels";
    private const string ExportDiffsHourlyRoute = "/api/export/diffs/hourly";
    private const string ExportGaugesHourlyRoute = "/api/export/gauges/hourly";

    [SetUp]
    public void SetUp()
    {
      seriesNameRepository = new Mock<ISeriesNameRepository>();
      exportRepository = new Mock<IExportRepository>();

      browser = new Browser(cfg =>
      {
        cfg.Module<ExportModule>();
        cfg.Dependency<ISeriesNameRepository>(seriesNameRepository.Object);
        cfg.Dependency<IExportRepository>(exportRepository.Object);
        cfg.Dependency<ILocationContext>(TimeZoneHelper.GetDenmarkLocationContext());
      });
    }

    [Test]
    public void GetLabels()
    {
      // Arrange
      var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterVolume1),
        new SeriesName("lbl2", ObisCode.ElectrActiveEnergyA14) };
      seriesNameRepository.Setup(pr => pr.GetStoredSeriesNames()).Returns(seriesNames);

      // Act
      var response = browser.Get(ExportLabelsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<string[]>();
      Assert.That(json, Is.EqualTo(seriesNames.Select(x => x.Label).ToArray()));
    }

    [Test]
    public void GetLabelsDistinctsLabels()
    {
      // Arrange
      var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterVolume1),
        new SeriesName("lbl1", ObisCode.ElectrActiveEnergyA14) };
      seriesNameRepository.Setup(pr => pr.GetStoredSeriesNames()).Returns(seriesNames);

      // Act
      var response = browser.Get(ExportLabelsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<string[]>();
      Assert.That(json, Is.EqualTo(new[] { "lbl1" } ));
    }

    [Test]
    public void GetLabelsIncludesCumulatives()
    {
      // Arrange
      var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterFlow1),
        new SeriesName("lbl2", ObisCode.ElectrActiveEnergyA14) };
      seriesNameRepository.Setup(pr => pr.GetStoredSeriesNames()).Returns(seriesNames);

      // Act
      var response = browser.Get(ExportLabelsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<string[]>();
      Assert.That(json, Is.EqualTo(new[] { "lbl2" }));
    }

    [Test]
    public void GetHourlyDiffExportFromAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportToAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportLabelsAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportFromBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", "BadFormat");
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportToBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", "BadFormat");
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportLabelsEmpty()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
        with.Query("lables", string.Empty);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyDiffExportFromToLabelsPresent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };
      var lss = new TimeRegisterValueLabelSeriesSet(today, today, new TimeRegisterValueLabelSeries[0]);
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.Is<DateTime>(dt => dt == today && dt.Kind == today.Kind),
                                                               It.Is<DateTime>(dt => dt == today.AddDays(1) && dt.Kind == today.Kind),
                                                               It.Is<IList<string>>(x => x.SequenceEqual(labels))));
    }

    [Test]
    public void GetHourlyDiffExportOneSeries()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ColdWaterVolume1;
      var ls = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t3, 5, 2, Unit.CubicMetre)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportDiffsRoot>();
      AssertPeriods(new[] { new ExportModule.Period(t1-skew, t2-skew), new ExportModule.Period(t2-skew, t3-skew) }, json.periods);

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportDiffSeries(ls.Label, obisCode.ToDelta(), new dynamic[] {
        new { From = t1, To = t2, Value = (double?)100, Unit = "m3" },
        new { From = t2, To = t3, Value = (double?)200, Unit = "m3" }
      }, json.series[0]);
    }

    [Test]
    public void GetHourlyDiffExportTwoSeriesOneLabel()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var a14 = ObisCode.ElectrActiveEnergyA14;
      var a23 = ObisCode.ElectrActiveEnergyA23;
      var ls1 = CreateLabelSeries("label1", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
      {
        { a14, new[]
          {
            new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
            new TimeRegisterValue("S1", t2, 7, 3, Unit.WattHour),
            new TimeRegisterValue("S1", t3, 10, 3, Unit.WattHour)
          }
        },
        {
          a23, new[]
          {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t3 + skew, 13, 3, Unit.WattHour)
          }
        }
      });

      var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls1 });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls1.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportDiffsRoot>();
      AssertPeriods(new[] { new ExportModule.Period(t1 - skew, t2 - skew), new ExportModule.Period(t2 - skew, t3 - skew) }, json.periods);

      Assert.That(json.series, Has.Length.EqualTo(4));
      AssertExportDiffSeries(ls1.Label, a14.ToDelta(), new dynamic[] {
        new { From = t1, To = t2, Value = 5, Unit = "kWh" },
        new { From = t2, To = t3, Value = 3, Unit = "kWh" },
      }, json.series[0]);
      AssertExportDiffSeries(ls1.Label, a23.ToDelta(), new dynamic[] {
        new { From = t1 + skew, To = t2 + skew, Value = 2, Unit = "kWh" },
        new { From = t2 + skew, To = t3 + skew, Value = 7, Unit = "kWh" },
      }, json.series[1]);
      AssertExportDiffSeries(ls1.Label, ObisCode.ElectrActiveEnergyA14NetDelta, new dynamic[] {
        new { From = t1, To = t2 + skew, Value = 3, Unit = "kWh" },
        new { From = t2, To = t3 + skew, Value = 0, Unit = "kWh" },
      }, json.series[2]);
      AssertExportDiffSeries(ls1.Label, ObisCode.ElectrActiveEnergyA23NetDelta, new dynamic[] {
        new { From = t1, To = t2 + skew, Value = 0, Unit = "kWh" },
        new { From = t2, To = t3 + skew, Value = 4, Unit = "kWh" },
      }, json.series[3]);
    }

    [Test]
    public void GetHourlyDiffExportTwoSeriesTwoLabels()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(2) + skew;
      var t2 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre)
        }
      );
      var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", new[] { ls1.Label, ls2.Label }));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportDiffsRoot>();
      AssertPeriods(new[] { new ExportModule.Period(t1 - skew, t2 - skew) }, json.periods);

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportDiffSeries(ls1.Label, obisCode.ToDelta(), new dynamic[] {
        new { From = t1, To = t2, Value = 100, Unit = "m3" },
      }, json.series[0]);
      AssertExportDiffSeries(ls2.Label, obisCode.ToDelta(), new dynamic[] {
        new { From = t1 + skew, To = t2 + skew, Value = 2, Unit = "kWh" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyDiffExportDiffValueAbsentWhenDeviceIdChanges()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
          new TimeRegisterValue("Other", t2, 3, 3, Unit.WattHour),
          new TimeRegisterValue("Other", t3, 4, 3, Unit.WattHour)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportDiffsHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportDiffsRoot>();
      AssertPeriods(new[] { new ExportModule.Period(t1 - skew, t2 - skew), new ExportModule.Period(t2 - skew, t3 - skew) }, json.periods);

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportDiffSeries(ls.Label, obisCode.ToDelta(), new dynamic[] {
        new { From = t1, To = t2, Value = 0, Unit = "kWh" },
        new { From = t2, To = t3, Value = 1, Unit = "kWh" },
      }, json.series[0]);
    }

    [Test]
    public void GetHourlyGaugesExportFromAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportToAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportLabelsAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportFromBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", "BadFormat");
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportToBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", "BadFormat");
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportLabelsEmpty()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
        with.Query("lables", string.Empty);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public void GetHourlyGaugesExportFromToLabelsPresent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };
      var lss = new TimeRegisterValueLabelSeriesSet(today, today, new TimeRegisterValueLabelSeries[0]);
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.ToString("o"));
        with.Query("to", today.AddDays(1).ToString("o"));
        with.Query("labels", string.Join(",", labels));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.Is<DateTime>(dt => dt == today && dt.Kind == today.Kind),
                                                               It.Is<DateTime>(dt => dt == today.AddDays(1) && dt.Kind == today.Kind),
                                                               It.Is<IList<string>>(x => x.SequenceEqual(labels))));
    }

    [Test]
    public void GetHourlyGaugesExportOneSeries()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ColdWaterVolume1;
      var ls = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t3, 5, 2, Unit.CubicMetre)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportGaugesRoot>();
      AssertTimestamps(new[] { t1, t2, t3 }.Select(x => x - skew), json.timestamps);

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportGaugeSeries(ls.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 200, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t2, Value = 300, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t3, Value = 500, Unit = "m3", DeviceId = "S1" }
      }, json.series[0]);
    }

    [Test]
    public void GetHourlyGaugesExportTwoSeriesOneLabel()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(2) + skew;
      var t2 = today - TimeSpan.FromHours(1) + skew;
      var obisCode1 = ObisCode.ElectrActiveEnergyA14;
      var obisCode2 = ObisCode.ElectrActiveEnergyA23;
      var ls1 = CreateLabelSeries("label1", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
      {
        { obisCode1, new[]
          {
            new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
            new TimeRegisterValue("S1", t2, 3, 3, Unit.WattHour)
          }
        },
        {
          obisCode2, new[]
          {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
          }
        }
      });

      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1 });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls1.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportGaugesRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportGaugeSeries(ls1.Label, obisCode1, new dynamic[] {
        new { Timestamp = t1, Value = 2, Unit = "kWh", DeviceId = "S1" },
        new { Timestamp = t2, Value = 3, Unit = "kWh", DeviceId = "S1" },
      }, json.series[0]);
      AssertExportGaugeSeries(ls1.Label, obisCode2, new dynamic[] {
        new { Timestamp = t1 + skew, Value = 4, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = t2 + skew, Value = 6, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyGaugesExportTwoSeriesTwoLabels()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(2) + skew;
      var t2 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre)
        }
      );
      var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", new[] { ls1.Label, ls2.Label }));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportGaugesRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportGaugeSeries(ls1.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 200, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t2, Value = 300, Unit = "m3", DeviceId = "S1" },
      }, json.series[0]);
      AssertExportGaugeSeries(ls2.Label, obisCode, new dynamic[] {
        new { Timestamp = t1 + skew, Value = 4, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = t2 + skew, Value = 6, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyGaugesExportValueAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t3, 3, 2, Unit.CubicMetre),
        }
      );
      var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1, 4, 3, Unit.WattHour),
            // t2 missing
            new TimeRegisterValue("S2", t3, 6, 3, Unit.WattHour)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportGaugesHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", new[] { ls1.Label, ls2.Label }));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportGaugesRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportGaugeSeries(ls2.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 4, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = (DateTime?)null, Value = (double?)null, Unit = (string)null, DeviceId = (string)null },
        new { Timestamp = t3, Value = 6, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    private static TimeRegisterValueLabelSeries CreateLabelSeries(string label, ObisCode obisCode, params TimeRegisterValue[] values)
    {
      return CreateLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { obisCode, values }
      });
    }

    private static TimeRegisterValueLabelSeries CreateLabelSeries(string label, IDictionary<ObisCode, IEnumerable<TimeRegisterValue>> values)
    {
      return new TimeRegisterValueLabelSeries(label, values);
    }

    private static void AssertTimestamps(IEnumerable<DateTime> expected, string[] actual)
    {
      Assert.That(actual, Is.EqualTo(expected.Select(x => x.ToUniversalTime().ToString("o").ToArray())));
    }

    private static void AssertPeriods(IEnumerable<ExportModule.Period> expected, ExportDiffPeriod[] actual)
    {
      Assert.That(actual.Select(x => new ExportModule.Period(DateTime.Parse(x.from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), DateTime.Parse(x.to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))).ToArray(), 
        Is.EqualTo(expected.ToArray()));
    }

    private static void AssertExportDiffSeries(string label, ObisCode obisCode, dynamic[] registers, ExportDiffSeries actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(label));
      Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
      Assert.That(actual.values, Has.Length.EqualTo(registers.Length));
      for (var ix = 0; ix < registers.Length; ix++)
      {
        var actualRegister = actual.values[ix];
        dynamic expectedRegister = registers[ix];
        Assert.That(actualRegister.from, Is.EqualTo(((DateTime?)expectedRegister.From).Value.ToUniversalTime().ToString("o")));
        Assert.That(actualRegister.to, Is.EqualTo(((DateTime?)expectedRegister.To).Value.ToUniversalTime().ToString("o")));
        Assert.That(actualRegister.value, Is.EqualTo(expectedRegister.Value));
        Assert.That(actualRegister.unit, Is.EqualTo(expectedRegister.Unit));
      }
    }

    private static void AssertExportGaugeSeries(string label, ObisCode obisCode, dynamic[] registers, ExportGaugeSeries actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(label));
      Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
      Assert.That(actual.values, Has.Length.EqualTo(registers.Length));
      for (var ix = 0; ix < registers.Length; ix++)
      {
        var actualRegister = actual.values[ix];
        dynamic expectedRegister = registers[ix];
        Assert.That(actualRegister.timestamp != null, Is.EqualTo(expectedRegister.Timestamp != null));
        if (expectedRegister.Timestamp != null)
        {
          Assert.That(actualRegister.timestamp, Is.EqualTo(((DateTime?)expectedRegister.Timestamp).Value.ToUniversalTime().ToString("o")));
        }
        Assert.That(actualRegister.value, Is.EqualTo(expectedRegister.Value));
        Assert.That(actualRegister.unit, Is.EqualTo(expectedRegister.Unit));
        Assert.That(actualRegister.deviceId, Is.EqualTo(expectedRegister.DeviceId));
      }
    }

    internal class ExportDiffsRoot
    {
      public ExportDiffPeriod[] periods { get; set; }
      public ExportDiffSeries[] series { get; set; }
    }

    internal class ExportDiffPeriod
    {
      public string from { get; set; }
      public string to { get; set; }
    }

    internal class ExportDiffSeries
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public ExportDiffRegister[] values { get; set; }
    }

    internal class ExportDiffRegister
    {
      public string from { get; set; }
      public string to { get; set; }
      public double? value { get; set; }
      public string unit { get; set; }
    }

    internal class ExportGaugesRoot
    {
      public string[] timestamps { get; set; }
      public ExportGaugeSeries[] series { get; set; }
    }

    internal class ExportGaugeSeries
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public ExportGaugeRegister[] values { get; set; }
    }

    internal class ExportGaugeRegister
    {
      public string timestamp { get; set; }
      public double? value { get; set; }
      public string unit { get; set; }
      public string deviceId { get; set; }
    }

  }
}
*/