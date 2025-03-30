using System;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
    [TestFixture]
    public class DeltaSeriesGeneratorTest
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

            var values = new[] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 };

            var target = new DeltaSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var deltaValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 10, unit, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 10, unit, "1"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, unit, "1"),
        Value(dt.AddHours(4), dt.AddHours(5), 10, unit, "1"),
        Value(dt.AddHours(5), dt.AddHours(6), 10, unit, "1"),
        Value(dt.AddHours(6), dt.AddHours(7), 40, unit, "1"),
        Value(dt.AddHours(7), dt.AddHours(8), 40, unit, "1"),
        Value(dt.AddHours(8), dt.AddHours(9), 10, unit, "1")
      };
            Assert.That(generatedValues, Is.EqualTo(deltaValues));
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

            var target = new DeltaSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var deltaValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, unit, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, unit, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, unit, "2"),
        Value(dt.AddHours(4), dt.AddHours(5), 10, unit, "2"),
        Value(dt.AddHours(5), dt.AddHours(6), 20, unit, "2"),
        Value(dt.AddHours(6), dt.AddHours(7), 0, unit, "2", "3"),
        Value(dt.AddHours(7), dt.AddHours(8), 10, unit, "3"),
        Value(dt.AddHours(8), dt.AddHours(9), 20, unit, "3")
      };
            Assert.That(generatedValues, Is.EqualTo(deltaValues));
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

            var target = new DeltaSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var deltaValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, unit, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, unit, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 10, unit, "2"),
        Value(dt.AddHours(4), dt.AddHours(5), 10, unit, "2"),
        Value(dt.AddHours(5), dt.AddHours(6), 20, unit, "2"),
        Value(dt.AddHours(6), dt.AddHours(7), 0, unit, "2", "1"),
        Value(dt.AddHours(7), dt.AddHours(8), 20, unit, "1"),
        Value(dt.AddHours(8), dt.AddHours(9), 10, unit, "1")
      };
            Assert.That(generatedValues, Is.EqualTo(deltaValues));
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

            var target = new DeltaSeriesGenerator();

            // Act
            foreach (var value in values)
            {
                target.CalculateNext(value);
            }
            var generatedValues = target.GetGeneratedDurations();

            // Assert
            var deltaValues = new[] {
        Value(dt, dt, 0, unit, "1"),
        Value(dt, dt.AddHours(1), 10, unit, "1"),
        Value(dt.AddHours(1), dt.AddHours(2), 20, unit, "1"),
        Value(dt.AddHours(2), dt.AddHours(3), 0, unit, "1", "2"),
        Value(dt.AddHours(3), dt.AddHours(4), 0, unit, "2", "1"),
        Value(dt.AddHours(4), dt.AddHours(5), 20, unit, "1"),
      };
            Assert.That(generatedValues, Is.EqualTo(deltaValues));
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
