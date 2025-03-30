using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class NormalizedDurationRegisterValueTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var dt = DateTime.UtcNow;
            var unitValue = new UnitValue();

            // Act & Assert
            Assert.That(() => new NormalizedDurationRegisterValue(DateTime.Now, dt, dt, dt, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new NormalizedDurationRegisterValue(dt, DateTime.Now, dt, dt, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new NormalizedDurationRegisterValue(dt, dt.Subtract(TimeSpan.FromMilliseconds(1)), dt, dt, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new NormalizedDurationRegisterValue(dt, dt, DateTime.Now, dt, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new NormalizedDurationRegisterValue(dt, dt, dt, DateTime.Now, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new NormalizedDurationRegisterValue(dt, dt, dt, dt.Subtract(TimeSpan.FromMilliseconds(1)), unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var end = start.AddHours(1);
            var normalizedStart = start.Subtract(TimeSpan.FromMinutes(1));
            var normalizedEnd = normalizedStart.AddHours(1);
            var unitValue = new UnitValue(1, 2, Unit.Joule);
            var deviceIds = new[] { "D1", "D2" };

            // Act
            var target = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds[0], deviceIds[1]);

            // Assert
            Assert.That(target.Start, Is.EqualTo(start));
            Assert.That(target.End, Is.EqualTo(end));
            Assert.That(target.NormalizedStart, Is.EqualTo(normalizedStart));
            Assert.That(target.NormalizedEnd, Is.EqualTo(normalizedEnd));
            Assert.That(target.UnitValue, Is.EqualTo(unitValue));
            Assert.That(target.DeviceIds, Is.EqualTo(deviceIds));
            Assert.That(target.OrderProperty, Is.EqualTo(end));
        }

        [Test]
        [TestCase(0, 0d)]
        [TestCase(8, 8d / 3600d)]
        [TestCase(-8, -8d / 3600d)]
        public void DurationDeviationRatio(int secondsDeviation, double expectedDeviation)
        {
            // Arrange
            var start = new DateTime(2024, 10, 13, 15, 0, 11, 0, DateTimeKind.Utc);
            var end = start.AddHours(1).AddSeconds(secondsDeviation);
            var normalizedStart = new DateTime(2024, 10, 13, 15, 0, 0, 0, DateTimeKind.Utc);
            var normalizedEnd = normalizedStart.AddHours(1);
            var target = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, new UnitValue(1, 2, Unit.Joule), "d1");

            // Act
            var deviation = target.DurationDeviationRatio;

            // Assert
            Assert.That(deviation, Is.EqualTo(expectedDeviation).Within(0.0000001));
        }

        [Test]
        [TestCase("D1", "d1", "D1")]
        [TestCase("d1", "D1", "d1")]
        public void DuplicateDeviceIds(string dId1, string dId2, string expected)
        {
            // Arrange
            var start = DateTime.UtcNow;
            var end = start.AddHours(1);
            var normalizedStart = start.Subtract(TimeSpan.FromMinutes(1));
            var normalizedEnd = normalizedStart.AddHours(1);
            var unitValue = new UnitValue(1, 2, Unit.Joule);
            IEnumerable<string> deviceIds = new[] { dId1, dId2 };

            // Act
            var target = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);

            // Assert
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { expected }));
        }


        [Test]
        [TestCase(0, 100, 100, 100)]
        [TestCase(1800, 100, 50, 100)]
        [TestCase(-900, 100, 100, 125)]
        public void GetDurationDeviationValue(int secondsDeviation, double value, double min, double max)
        {
            // Arrange
            var start = new DateTime(2024, 10, 13, 15, 0, 11, 0, DateTimeKind.Utc);
            var end = start.AddHours(1).AddSeconds(secondsDeviation);
            var normalizedStart = new DateTime(2024, 10, 13, 15, 0, 0, 0, DateTimeKind.Utc);
            var normalizedEnd = normalizedStart.AddHours(1);
            var target = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, new UnitValue(value, Unit.Joule), "d1");

            // Act
            var deviation = target.GetDurationDeviationValue();

            // Assert
            var expectedDeviation = new DeviationValue(value, min, max);
            Assert.That(deviation, Is.EqualTo(expectedDeviation));
        }

        [Test]
        public void SubtractNotNegative()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 31, 00, DateTimeKind.Utc);
            var t1 = GetTarget(dt.AddMinutes(1), 20, Unit.Watt, deviceIds: new string[] { "DID1" });
            var t2 = GetTarget(dt, 21, Unit.Watt, deviceIds: new string[] { "did2" });

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.Start, Is.EqualTo(dt));
            Assert.That(target.End, Is.EqualTo(dt.AddMinutes(6)));
            Assert.That(target.NormalizedStart, Is.EqualTo(NormalizeTimestamp(dt)));
            Assert.That(target.NormalizedEnd, Is.EqualTo(NormalizeTimestamp(dt.AddMinutes(5))));
            Assert.That(target.UnitValue, Is.EqualTo(new UnitValue(1, Unit.Watt)));
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { "did2", "DID1" }));
        }

        [Test]
        public void SubtractNotNegativeNegativeToZero()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var t1 = GetTarget(dt, 22, Unit.Watt);
            var t2 = GetTarget(dt, 21, Unit.Watt);

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.UnitValue.Value, Is.Zero);
        }

        [Test]
        public void SubtractNotNegativeDifferentUnitsThrows()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var t1 = GetTarget(dt, 20, Unit.Watt);
            var t2 = GetTarget(dt, 21, Unit.Joule);

            // Act & Assert
            Assert.That(() => t1.SubtractNotNegative(t2), Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            var start = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var end = start.AddHours(1);
            var normalizedStart = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var normalizedEnd = normalizedStart.AddHours(1);
            var unitValue = new UnitValue(1, 2, Unit.Joule);
            var deviceIds = new[] { "D1", "D2" };

            // Act
            var target = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);

            // Assert
            Assert.That(target.ToString(), Is.EqualTo("[start=2015-02-13T19:30:00.0000000Z, end=2015-02-13T20:30:00.0000000Z, normalizedStart=2015-02-13T19:00:00.0000000Z, normalizedEnd=2015-02-13T20:00:00.0000000Z, unitValue=[value=100, unit=Joule], deviceIds=[D1, D2]]"));
        }

        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var start = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var end = start.AddHours(1);
            var normalizedStart = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var normalizedEnd = normalizedStart.AddHours(1);
            var unitValue = new UnitValue(1, 2, Unit.Joule);
            var deviceIds = new[] { "D1", "D2" };
            var t1 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t2 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t3 = new NormalizedDurationRegisterValue(start.AddMinutes(1), end, normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t4 = new NormalizedDurationRegisterValue(start, end.AddMinutes(1), normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t5 = new NormalizedDurationRegisterValue(start, end, normalizedStart.AddMinutes(1), normalizedEnd, unitValue, deviceIds);
            var t6 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd.AddMinutes(1), unitValue, deviceIds);
            var t7 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, new UnitValue(), deviceIds);
            var t8 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, new string[] { "D3" });

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t4));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t5));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t6));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t6.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t7));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t7.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t8));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t8.GetHashCode()));
        }

        [Test]
        public void Equeality()
        {
            // Arrange
            var start = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var end = start.AddHours(1);
            var normalizedStart = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var normalizedEnd = normalizedStart.AddHours(1);
            var unitValue = new UnitValue(1, 2, Unit.Joule);
            var deviceIds = new[] { "D1", "D2" };
            var t1 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t2 = new NormalizedDurationRegisterValue(start, end, normalizedStart, normalizedEnd, unitValue, deviceIds);
            var t3 = new NormalizedDurationRegisterValue(start.AddMinutes(1), end, normalizedStart, normalizedEnd, unitValue, deviceIds);

            // Act & Assert
            Assert.That(t1 == t2, Is.True);
            Assert.That(t1 == t3, Is.False);
        }

        private static DateTime NormalizeTimestamp(DateTime timestamp, string interval = "5-minutes")
        {
            var dateTimeHelper = GetDateTimeHelper();
            var timeDivider = dateTimeHelper.GetDivider(interval);
            return timeDivider(timestamp);
        }

        private static NormalizedDurationRegisterValue GetTarget(DateTime timestamp, double value, Unit unit, string interval = "5-minutes", params string[] deviceIds)
        {
            var dateTimeHelper = GetDateTimeHelper();
            var end = dateTimeHelper.GetNext(interval)(timestamp);
            var timeDivider = dateTimeHelper.GetDivider(interval);
            return new NormalizedDurationRegisterValue(timestamp, end, NormalizeTimestamp(timestamp, interval), NormalizeTimestamp(end, interval),
              new UnitValue(value, unit), deviceIds);
        }

        private static DateTimeHelper GetDateTimeHelper()
        {
            var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
            var dateTimeHelper = new DateTimeHelper(locationContext, new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime());
            return dateTimeHelper;
        }


    }
}

