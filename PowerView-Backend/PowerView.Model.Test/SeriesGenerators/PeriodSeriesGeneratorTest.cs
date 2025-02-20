using System;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
  [TestFixture]
  public class PeriodSeriesGeneratorTest
  {
    [Test]
    public void GenerateSameDeviceId()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, unit));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 212, 1, unit));
      var sv3 = Normalize(new TimeRegisterValue("1", dt.AddHours(3), 213, 1, unit));
      var sv4 = Normalize(new TimeRegisterValue("1", dt.AddHours(4), 214, 1, unit));
      var sv5 = Normalize(new TimeRegisterValue("1", dt.AddHours(5), 215, 1, unit));
      var sv6 = Normalize(new TimeRegisterValue("1", dt.AddHours(6), 216, 1, unit));
      var sv7 = Normalize(new TimeRegisterValue("1", dt.AddHours(7), 220, 1, unit));
      var sv8 = Normalize(new TimeRegisterValue("1", dt.AddHours(8), 224, 1, unit));
      var sv9 = Normalize(new TimeRegisterValue("1", dt.AddHours(9), 225, 1, unit));

      var values = new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGeneratedDurations();

      // Assert
      var periodValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt, dt.AddHours(2), 20, unit, "1"),
        Value(dt, dt.AddHours(3), 30, unit, "1"),
        Value(dt, dt.AddHours(4), 40, unit, "1"),
        Value(dt, dt.AddHours(5), 50, unit, "1"),
        Value(dt, dt.AddHours(6), 60, unit, "1"),
        Value(dt, dt.AddHours(7), 100, unit, "1"),
        Value(dt, dt.AddHours(8), 140, unit, "1"),
        Value(dt, dt.AddHours(9), 150, unit, "1")
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingDeviceId()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, unit));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit));
      var sv4 = Normalize(new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit));
      var sv5 = Normalize(new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit));
      var sv6 = Normalize(new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit));
      var sv7 = Normalize(new TimeRegisterValue("3", dt.AddHours(7), 101, 1, unit));
      var sv8 = Normalize(new TimeRegisterValue("3", dt.AddHours(8), 102, 1, unit));
      var sv9 = Normalize(new TimeRegisterValue("3", dt.AddHours(9), 104, 1, unit));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGeneratedDurations();

      // Assert
      var periodValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt, dt.AddHours(2), 30, unit, "1"),
        Value(dt, dt.AddHours(3), 30+0, unit, "1", "2"),
        Value(dt, dt.AddHours(4), 30+10, unit, "1", "2"),
        Value(dt, dt.AddHours(5), 30+20, unit, "1", "2"),
        Value(dt, dt.AddHours(6), 30+40, unit, "1", "2"),
        Value(dt, dt.AddHours(7), 30+40+0, unit, "1", "2", "3"),
        Value(dt, dt.AddHours(8), 30+40+10, unit, "1", "2", "3"),
        Value(dt, dt.AddHours(9), 30+40+30, unit, "1", "2", "3")
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingDeviceIdAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, unit));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit));
      var sv4 = Normalize(new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit));
      var sv5 = Normalize(new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit));
      var sv6 = Normalize(new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit));
      var sv7 = Normalize(new TimeRegisterValue("1", dt.AddHours(7), 215, 1, unit));
      var sv8 = Normalize(new TimeRegisterValue("1", dt.AddHours(8), 217, 1, unit));
      var sv9 = Normalize(new TimeRegisterValue("1", dt.AddHours(9), 218, 1, unit));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGeneratedDurations();

      // Assert
      var periodValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt, dt.AddHours(2), 30, unit, "1"),
        Value(dt, dt.AddHours(3), 30+0, unit, "1", "2"),
        Value(dt, dt.AddHours(4), 30+10, unit, "1", "2"),
        Value(dt, dt.AddHours(5), 30+20, unit, "1", "2"),
        Value(dt, dt.AddHours(6), 30+40, unit, "1", "2"),
        Value(dt, dt.AddHours(7), 30+40+0, unit, "1", "2"),
        Value(dt, dt.AddHours(8), 30+40+20, unit, "1", "2"),
        Value(dt, dt.AddHours(9), 30+40+30, unit, "1", "2")
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingDeviceIdWithOneValueAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, unit));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit));
      var sv4 = Normalize(new TimeRegisterValue("1", dt.AddHours(4), 215, 1, unit));
      var sv5 = Normalize(new TimeRegisterValue("1", dt.AddHours(5), 217, 1, unit));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGeneratedDurations();

      // Assert
      var periodValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt, dt.AddHours(2), 30, unit, "1"),
        Value(dt, dt.AddHours(3), 30+0, unit, "1", "2"),
        Value(dt, dt.AddHours(4), 30+0+0, unit, "1", "2"),
        Value(dt, dt.AddHours(5), 30+0+20, unit, "1", "2")
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "5-minutes")
    {
      var timeDivider = GetDivider();
      return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
    }

    private static NormalizedDurationRegisterValue Value(DateTime start, DateTime end, double value, Unit unit, params string[] deviceIds)
    {
      var timeDivider = GetDivider();
      return new NormalizedDurationRegisterValue(start, end, timeDivider(start), timeDivider(end), new UnitValue(value, unit), deviceIds);
    }

    private static Func<DateTime, DateTime> GetDivider(string interval = "5-minutes")
    {
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var dateTimeHelper = new DateTimeHelper(locationContext, new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return timeDivider;
    }

  }
}
