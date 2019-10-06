using System;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
  [TestFixture]
  public class AverageActualSeriesGeneratorTest
  {
    [Test]
    public void GenerateSameSerialNumber()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 960936, 1, wh));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, wh));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(10), 960952, 1, wh));
      var sv3 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(15), 960960, 1, wh));
      var sv4 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(20), 960968, 1, wh));
      var sv5 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(25), 960977, 1, wh));
      var sv6 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(30), 960985, 1, wh));
      var sv7 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(35), 960992, 1, wh));
      var sv8 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(40), 961000, 1, wh));
      var sv9 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(45), 961007, 1, wh));

      var values = new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new AverageActualSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var w = Unit.Watt;
      var averageValues = new[] {
        Normalize(new TimeRegisterValue("1", dt, 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(5), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(10), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(15), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(20), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(25), 1080, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(30), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(35), 840, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(40), 960, w)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(45), 840, w)),
      };
      Assert.That(generatedValues, Is.EqualTo(averageValues));
    }

    [Test]
    public void GenerateCrossingSerialNumber()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, wh));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, wh));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, wh));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, wh));
      var sv4 = Normalize(new TimeRegisterValue("2", dt.AddHours(4), 302, 1, wh));
      var sv5 = Normalize(new TimeRegisterValue("2", dt.AddHours(5), 303, 1, wh));
      var sv6 = Normalize(new TimeRegisterValue("2", dt.AddHours(6), 305, 1, wh));
      var sv7 = Normalize(new TimeRegisterValue("3", dt.AddHours(7), 101, 1, wh));
      var sv8 = Normalize(new TimeRegisterValue("3", dt.AddHours(8), 102, 1, wh));
      var sv9 = Normalize(new TimeRegisterValue("3", dt.AddHours(9), 104, 1, wh));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new AverageActualSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var w = Unit.Watt;
      var averageValues = new[] {
        Normalize(new TimeRegisterValue("1", dt, 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(1), 10, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(2), 20, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(3), 0, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(4), 10, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(5), 10, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(6), 20, w)),
        Normalize(new TimeRegisterValue("3", dt.AddHours(7), 0, w)),
        Normalize(new TimeRegisterValue("3", dt.AddHours(8), 10, w)),
        Normalize(new TimeRegisterValue("3", dt.AddHours(9), 20, w)),
      };
      Assert.That(generatedValues, Is.EqualTo(averageValues));
    }

    [Test]
    public void GenerateCrossingSerialNumberAndBack()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, wh));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, wh));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, wh));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, wh));
      var sv4 = Normalize(new TimeRegisterValue("2", dt.AddHours(4), 302, 1, wh));
      var sv5 = Normalize(new TimeRegisterValue("2", dt.AddHours(5), 303, 1, wh));
      var sv6 = Normalize(new TimeRegisterValue("2", dt.AddHours(6), 305, 1, wh));
      var sv7 = Normalize(new TimeRegisterValue("1", dt.AddHours(7), 215, 1, wh));
      var sv8 = Normalize(new TimeRegisterValue("1", dt.AddHours(8), 217, 1, wh));
      var sv9 = Normalize(new TimeRegisterValue("1", dt.AddHours(9), 218, 1, wh));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new AverageActualSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var w = Unit.Watt;
      var averageValues = new[] {
        Normalize(new TimeRegisterValue("1", dt, 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(1), 10, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(2), 20, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(3), 0, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(4), 10, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(5), 10, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(6), 20, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(7), 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(8), 20, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(9), 10, w)),
      };
      Assert.That(generatedValues, Is.EqualTo(averageValues));
    }

    [Test]
    public void GenerateCrossingSerialNumberWithOneValueAndBack()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, wh));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, wh));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, wh));
      var sv3 = Normalize(new TimeRegisterValue("2", dt.AddHours(3), 301, 1, wh));
      var sv4 = Normalize(new TimeRegisterValue("1", dt.AddHours(4), 215, 1, wh));
      var sv5 = Normalize(new TimeRegisterValue("1", dt.AddHours(5), 217, 1, wh));

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5 };

      var target = new AverageActualSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var w = Unit.Watt;
      var averageValues = new[] {
        Normalize(new TimeRegisterValue("1", dt, 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(1), 10, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(2), 20, w)),
        Normalize(new TimeRegisterValue("2", dt.AddHours(3), 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(4), 0, w)),
        Normalize(new TimeRegisterValue("1", dt.AddHours(5), 20, w)),
      };
      Assert.That(generatedValues, Is.EqualTo(averageValues));
    }

    [Test]
    [TestCase(Unit.WattHour, Unit.Watt)]
    [TestCase(Unit.CubicMetre, Unit.CubicMetrePrHour)]
    public void GenerateWithUnits(Unit sourceUnit, Unit expectedUnit)
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = Normalize(new TimeRegisterValue("1", dt, 960936, 1, sourceUnit));
      var sv1 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, sourceUnit));
      var sv2 = Normalize(new TimeRegisterValue("1", dt.AddMinutes(10), 960952, 1, sourceUnit));

      var values = new[] { sv0, sv1, sv2 };

      var target = new AverageActualSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var averageValues = new[] {
        Normalize(new TimeRegisterValue("1", dt, 0, expectedUnit)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(5), 960, expectedUnit)),
        Normalize(new TimeRegisterValue("1", dt.AddMinutes(10), 960, expectedUnit))
      };
      Assert.That(generatedValues, Is.EqualTo(averageValues));
    }

    private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "5-minutes")
    {
      var dateTimeHelper = new DateTimeHelper(TimeZoneInfo.Local, new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
    }

  }
}
