using System;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
    [TestFixture]
    public class AverageActualSeriesGeneratorTest
    {
        [Test]
        public void GenerateSameDeviceId()
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

            var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

            var target = new AverageActualSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var w = Unit.Watt;
            var averageValues = new[] {
        Value(dt, dt, 0, w, "1"),
        Value(dt, dt.AddMinutes(5), 960, w, "1"),
        Value(dt.AddMinutes(5), dt.AddMinutes(10), 960, w, "1"),
        Value(dt.AddMinutes(10), dt.AddMinutes(15), 960, w, "1"),
        Value(dt.AddMinutes(15), dt.AddMinutes(20), 960, w, "1"),
        Value(dt.AddMinutes(20), dt.AddMinutes(25), 1080, w, "1"),
        Value(dt.AddMinutes(25), dt.AddMinutes(30), 960, w, "1"),
        Value(dt.AddMinutes(30), dt.AddMinutes(35), 840, w, "1"),
        Value(dt.AddMinutes(35), dt.AddMinutes(40), 960, w, "1"),
        Value(dt.AddMinutes(40), dt.AddMinutes(45), 840, w, "1")
      };
            Assert.That(generatedValues, Is.EqualTo(averageValues));
        }

        [Test]
        public void GenerateCrossingDeviceId()
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
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var w = Unit.Watt;
            var averageValues = new[] {
        Value(dt, dt, 0, w, "1"),
        Value(dt, dt.AddHours(1), 10, w, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, w, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, w, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, w, "2"),
        Value(dt.AddHours(4), dt.AddHours(5), 10, w, "2"),
        Value(dt.AddHours(5), dt.AddHours(6), 20, w, "2"),
        Value(dt.AddHours(6), dt.AddHours(7), 0, w, "2", "3"),
        Value(dt.AddHours(7), dt.AddHours(8), 10, w, "3"),
        Value(dt.AddHours(8), dt.AddHours(9), 20, w, "3")
      };
            Assert.That(generatedValues, Is.EqualTo(averageValues));
        }

        [Test]
        public void GenerateCrossingDeviceIdAndBack()
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
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var w = Unit.Watt;
            var averageValues = new[] {
        Value(dt, dt, 0, w, "1"),
        Value(dt, dt.AddHours(1), 10, w, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, w, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, w, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, w, "2"),
        Value(dt.AddHours(4), dt.AddHours(5), 10, w, "2"),
        Value(dt.AddHours(5), dt.AddHours(6), 20, w, "2"),
        Value(dt.AddHours(6), dt.AddHours(7), 0, w, "2", "1"),
        Value(dt.AddHours(7), dt.AddHours(8), 20, w, "1"),
        Value(dt.AddHours(8), dt.AddHours(9), 10, w, "1")
      };
            Assert.That(generatedValues, Is.EqualTo(averageValues));
        }

        [Test]
        public void GenerateCrossingDeviceIdWithOneValueAndBack()
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
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var w = Unit.Watt;
            var averageValues = new[] {
        Value(dt, dt, 0, w, "1"),
        Value(dt, dt.AddHours(1), 10, w, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, w, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, w, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 0, w, "2", "1"),
        Value(dt.AddHours(4), dt.AddHours(5), 20, w, "1"),
      };
            Assert.That(generatedValues, Is.EqualTo(averageValues));
        }

        [Test]
        public void GenerateCrossingUnit()
        {
            // Arrange
            var wh = Unit.WattHour;
            var m3 = Unit.CubicMetre; // Just an example of a different unit - although it doesn't make sense to mix energy and volume.
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            var sv0 = Normalize(new TimeRegisterValue("1", dt, 210, 1, wh));
            var sv1 = Normalize(new TimeRegisterValue("1", dt.AddHours(1), 211, 1, wh));
            var sv2 = Normalize(new TimeRegisterValue("1", dt.AddHours(2), 213, 1, wh));
            var sv3 = Normalize(new TimeRegisterValue("1", dt.AddHours(3), 301, 1, m3));
            var sv4 = Normalize(new TimeRegisterValue("1", dt.AddHours(4), 302, 1, m3));
            var sv5 = Normalize(new TimeRegisterValue("1", dt.AddHours(5), 304, 1, m3));

            var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5 };

            var target = new AverageActualSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var w = Unit.Watt;
            var m3h = Unit.CubicMetrePrHour;
            var averageValues = new[] {
        Value(dt, dt, 0, w, "1"),
        Value(dt, dt.AddHours(1), 10, w, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, w, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, m3h, "1"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, m3h, "1"),
        Value(dt.AddHours(4), dt.AddHours(5), 20, m3h, "1"),
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
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var averageValues = new[] {
        Value(dt, dt, 0, expectedUnit, "1"),
        Value(dt, dt.AddMinutes(5), 960, expectedUnit, "1"),
        Value(dt.AddMinutes(5), dt.AddMinutes(10), 960, expectedUnit, "1")
      };
            Assert.That(generatedValues, Is.EqualTo(averageValues));
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
