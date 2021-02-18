using System;
using System.Collections.Generic;
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
    private const string ExportHourlyRoute = "/api/export/hourly";

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
    public void GetHourlyExportFromAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportToAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportLabelsAbsent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportFromBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportToBadFormat()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportLabelsEmpty()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportFromToLabelsPresent()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labels = new[] { "lbl1", "lbl2" };
      var lss = new TimeRegisterValueLabelSeriesSet(today, today, new TimeRegisterValueLabelSeries[0]);
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
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
    public void GetHourlyExportOneSeries()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(3) + skew;
      var t2 = today - TimeSpan.FromHours(2) + skew;
      var t3 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ColdWaterVolume1;
      var ls = CreateLabelSeries("label1", obisCode, new []
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre), 
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t3, 5, 2, Unit.CubicMetre)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();
      AssertTimestamps(new[] { t1, t2, t3 }.Select(x => x - skew), json.timestamps);

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportSeries(ls.Label, obisCode, new dynamic[] { 
        new { Timestamp = t1, Value = 200, DiffValue = (double?)null, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t2, Value = 300, DiffValue = (double?)100, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t3, Value = 500, DiffValue = (double?)200, Unit = "m3", DeviceId = "S1" }
      }, json.series[0]);
    }

    [Test]
    public void GetHourlyExportTwoSeriesOneLabel()
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
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls1.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportSeries(ls1.Label, obisCode1, new dynamic[] {
        new { Timestamp = t1, Value = 2, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S1" },
        new { Timestamp = t2, Value = 3, DiffValue = 1, Unit = "kWh", DeviceId = "S1" },
      }, json.series[0]);
      AssertExportSeries(ls1.Label, obisCode2, new dynamic[] {
        new { Timestamp = t1 + skew, Value = 4, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = t2 + skew, Value = 6, DiffValue = 2, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyExportTwoSeriesTwoLabels()
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
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", new[] { ls1.Label, ls2.Label }));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportSeries(ls1.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 200, DiffValue = (double?)null, Unit = "m3", DeviceId = "S1" },
        new { Timestamp = t2, Value = 300, DiffValue = 100, Unit = "m3", DeviceId = "S1" },
      }, json.series[0]);
      AssertExportSeries(ls2.Label, obisCode, new dynamic[] {
        new { Timestamp = t1 + skew, Value = 4, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = t2 + skew, Value = 6, DiffValue = 2, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyExportValueAbsent()
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
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", string.Join(",", new[] { ls1.Label, ls2.Label }));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();

      Assert.That(json.series, Has.Length.EqualTo(2));
      AssertExportSeries(ls2.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 4, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = (DateTime?)null, Value = (double?)null, DiffValue = (double?)null, Unit = (string)null, DeviceId = (string)null },
        new { Timestamp = t3, Value = 6, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S2" },
      }, json.series[1]);
    }

    [Test]
    public void GetHourlyExportDiffValueAbsentWhenUnitChanges()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(2) + skew;
      var t2 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
          new TimeRegisterValue("S1", t2, 277777777, 2, Unit.Joule)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportSeries(ls.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 2, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S1" },
        new { Timestamp = t2, Value = 7716.049, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S1" },
      }, json.series[0]);
    }

    [Test]
    public void GetHourlyExportDiffValueAbsentWhenDeviceIdChanges()
    {
      // Arrange
      var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var skew = TimeSpan.FromMinutes(5); // To verify normalization.
      var t1 = today - TimeSpan.FromHours(2) + skew;
      var t2 = today - TimeSpan.FromHours(1) + skew;
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var ls = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
          new TimeRegisterValue("Other", t2, 3, 3, Unit.WattHour)
        }
      );
      var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls });
      exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

      // Act
      var response = browser.Get(ExportHourlyRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", today.AddHours(-5).ToString("o"));
        with.Query("to", today.ToString("o"));
        with.Query("labels", ls.Label);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<ExportRoot>();

      Assert.That(json.series, Has.Length.EqualTo(1));
      AssertExportSeries(ls.Label, obisCode, new dynamic[] {
        new { Timestamp = t1, Value = 2, DiffValue = (double?)null, Unit = "kWh", DeviceId = "S1" },
        new { Timestamp = t2, Value = 3, DiffValue = (double?)null, Unit = "kWh", DeviceId = "Other" },
      }, json.series[0]);
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

    private static void AssertExportSeries(string label, ObisCode obisCode, dynamic[] registers, ExportSeries actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.label, Is.EqualTo(label));
      Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
      Assert.That(actual.values, Has.Length.EqualTo(registers.Length));
      for (var ix=0; ix < registers.Length; ix++)
      {
        var actualRegister = actual.values[ix];
        dynamic expectedRegister = registers[ix];
        Assert.That(actualRegister.timestamp != null, Is.EqualTo(expectedRegister.Timestamp != null));
        if (expectedRegister.Timestamp != null)
        {
          Assert.That(actualRegister.timestamp, Is.EqualTo(((DateTime?)expectedRegister.Timestamp).Value.ToUniversalTime().ToString("o")));
        }
        Assert.That(actualRegister.value, Is.EqualTo(expectedRegister.Value));
        Assert.That(actualRegister.diffValue, Is.EqualTo(expectedRegister.DiffValue));
        Assert.That(actualRegister.unit, Is.EqualTo(expectedRegister.Unit));
        Assert.That(actualRegister.deviceId, Is.EqualTo(expectedRegister.DeviceId));
      }
    }

    internal class ExportRoot
    {
      public string[] timestamps { get; set; }
      public ExportSeries[] series { get; set; }
    }

    internal class ExportSeries
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public ExportRegister[] values { get; set; }
    }

    internal class ExportRegister
    {
      public string timestamp { get; set; }
      public double? value { get; set; }
      public double? diffValue { get; set; }
      public string unit { get; set; }
      public string deviceId { get; set; }
    }

  }
}
