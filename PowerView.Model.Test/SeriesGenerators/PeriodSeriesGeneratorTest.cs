using System;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
  [TestFixture]
  public class PeriodSeriesGeneratorTest
  {
    [Test]
    public void GenerateSameSerialNumber()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 212, 1, unit);
      var sv3 = new TimeRegisterValue("1", dt.AddHours(3), 213, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(4), 214, 1, unit);
      var sv5 = new TimeRegisterValue("1", dt.AddHours(5), 215, 1, unit);
      var sv6 = new TimeRegisterValue("1", dt.AddHours(6), 216, 1, unit);
      var sv7 = new TimeRegisterValue("1", dt.AddHours(7), 220, 1, unit);
      var sv8 = new TimeRegisterValue("1", dt.AddHours(8), 224, 1, unit);
      var sv9 = new TimeRegisterValue("1", dt.AddHours(9), 225, 1, unit);

      var values = new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 20, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 30, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 40, unit),
        new TimeRegisterValue("1", dt.AddHours(5), 50, unit),
        new TimeRegisterValue("1", dt.AddHours(6), 60, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 100, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 140, unit),
        new TimeRegisterValue("1", dt.AddHours(9), 150, unit),
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingSerialNumber()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit);
      var sv5 = new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit);
      var sv6 = new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit);
      var sv7 = new TimeRegisterValue("3", dt.AddHours(7), 101, 1, unit);
      var sv8 = new TimeRegisterValue("3", dt.AddHours(8), 102, 1, unit);
      var sv9 = new TimeRegisterValue("3", dt.AddHours(9), 104, 1, unit);

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(4), 30+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(5), 30+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(6), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+40+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(9), 30+40+30, unit)
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingSerialNumberAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit);
      var sv5 = new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit);
      var sv6 = new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit);
      var sv7 = new TimeRegisterValue("1", dt.AddHours(7), 215, 1, unit);
      var sv8 = new TimeRegisterValue("1", dt.AddHours(8), 217, 1, unit);
      var sv9 = new TimeRegisterValue("1", dt.AddHours(9), 218, 1, unit);

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(4), 30+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(5), 30+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(6), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+40+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(9), 30+40+30, unit)
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

    [Test]
    public void GenerateCrossingSerialNumberWithOneValueAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(7), 215, 1, unit);
      var sv5 = new TimeRegisterValue("1", dt.AddHours(8), 217, 1, unit);

      var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5 };

      var target = new PeriodSeriesGenerator();

      // Act
      foreach (var value in values)
      {
        target.CalculateNext(value);
      }
      var generatedValues = target.GetGenerated();

      // Assert
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+0+20, unit)
      };
      Assert.That(generatedValues, Is.EqualTo(periodValues));
    }

  }
}
