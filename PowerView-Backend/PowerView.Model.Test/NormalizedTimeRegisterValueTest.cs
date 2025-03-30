using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class NormalizedTimeRegisterValueTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var timeRegisterValue = new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue());

            // Act & Assert
            Assert.That(() => new NormalizedTimeRegisterValue(timeRegisterValue, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var timeRegisterValue = new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue());
            var dt = DateTime.UtcNow;

            // Act
            var target = new NormalizedTimeRegisterValue(timeRegisterValue, dt);

            // Assert
            Assert.That(target.TimeRegisterValue, Is.EqualTo(timeRegisterValue));
            Assert.That(target.NormalizedTimestamp, Is.EqualTo(dt));
            Assert.That(target.OrderProperty, Is.EqualTo(timeRegisterValue.Timestamp));
        }

        [Test]
        public void SubtractAccommodateWrap()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 21, 1, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractAccommodateWrap(t1);

            // Assert
            Assert.That(target.Start, Is.EqualTo(t1.TimeRegisterValue.Timestamp));
            Assert.That(target.End, Is.EqualTo(t2.TimeRegisterValue.Timestamp));
            Assert.That(target.NormalizedStart, Is.EqualTo(t1.NormalizedTimestamp));
            Assert.That(target.NormalizedEnd, Is.EqualTo(t2.NormalizedTimestamp));
            Assert.That(target.UnitValue, Is.EqualTo(new UnitValue(10, Unit.Watt)));
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { "DID" }));
        }

        [Test]
        public void SubtractAccommodateWrapDeviceIdCaseInsensitive()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("did", dt, 21, 1, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractAccommodateWrap(t1);

            // Assert
            Assert.That(target.Start, Is.EqualTo(t1.TimeRegisterValue.Timestamp));
            Assert.That(target.End, Is.EqualTo(t2.TimeRegisterValue.Timestamp));
            Assert.That(target.NormalizedStart, Is.EqualTo(t1.NormalizedTimestamp));
            Assert.That(target.NormalizedEnd, Is.EqualTo(t2.NormalizedTimestamp));
            Assert.That(target.UnitValue, Is.EqualTo(new UnitValue(10, Unit.Watt)));
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { "did" }));
        }

        [Test]
        public void SubtractAccommodateWrapNegativeAssumeRegisterQuirk()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 102, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 101, 0, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractAccommodateWrap(t1);

            // Assert
            Assert.That(target.UnitValue.Value, Is.EqualTo(0));
        }

        [Test]
        public void SubtractAccommodateWrapNegativeAssumeRegisterWrap()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 880, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 10, 0, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractAccommodateWrap(t1);

            // Assert
            Assert.That(target.UnitValue.Value, Is.EqualTo(1000 - 880 + 10));
        }

        [Test]
        public void SubtractAccommodateWrapNegativeThrows()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 880, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 600, 0, Unit.Watt), dtNorm);

            // Act & Assert
            Assert.That(() => t2.SubtractAccommodateWrap(t1), Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        public void SubtractAccommodateWrapCrossDeviceIdsThrows()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID1", dt.AddMinutes(-5), 600, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID2", dt, 600, 0, Unit.Watt), dtNorm);

            // Act & Assert
            Assert.That(() => t2.SubtractAccommodateWrap(t1), Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        public void SubtractAccommodateWrapDifferentUnitsThrows()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 21, 1, Unit.WattHour), dtNorm);

            // Act & Assert
            Assert.That(() => t1.SubtractAccommodateWrap(t2), Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        public void SubtractNotNegative()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 21, 1, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.Start, Is.EqualTo(t1.TimeRegisterValue.Timestamp));
            Assert.That(target.End, Is.EqualTo(t2.TimeRegisterValue.Timestamp));
            Assert.That(target.NormalizedStart, Is.EqualTo(t1.NormalizedTimestamp));
            Assert.That(target.NormalizedEnd, Is.EqualTo(t2.NormalizedTimestamp));
            Assert.That(target.UnitValue, Is.EqualTo(new UnitValue(10, Unit.Watt)));
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { "DID" }));
        }

        [Test]
        public void SubtractNotNegativeDeviceIdCaseInsensitive()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("did", dt, 21, 1, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.Start, Is.EqualTo(t1.TimeRegisterValue.Timestamp));
            Assert.That(target.End, Is.EqualTo(t2.TimeRegisterValue.Timestamp));
            Assert.That(target.NormalizedStart, Is.EqualTo(t1.NormalizedTimestamp));
            Assert.That(target.NormalizedEnd, Is.EqualTo(t2.NormalizedTimestamp));
            Assert.That(target.UnitValue, Is.EqualTo(new UnitValue(10, Unit.Watt)));
            Assert.That(target.DeviceIds, Is.EqualTo(new[] { "did" }));
        }

        [Test]
        public void SubtractNotNegativeNegativeToZero()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 880, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 10, 0, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.UnitValue.Value, Is.Zero);
        }

        [Test]
        public void SubtractNotNegativeCrossDeviceIds()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID1", dt.AddMinutes(-5), 600, 0, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID2", dt, 700, 0, Unit.Watt), dtNorm);

            // Act
            var target = t2.SubtractNotNegative(t1);

            // Assert
            Assert.That(target.UnitValue.Value, Is.EqualTo(100));
        }

        [Test]
        public void SubtractNotNegativeDifferentUnitsThrows()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dtNorm = new DateTime(2015, 02, 13, 19, 00, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt.AddMinutes(-5), 20, 1, Unit.Watt), dtNorm.AddMinutes(-5));
            var t2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("DID", dt, 21, 1, Unit.WattHour), dtNorm);

            // Act & Assert
            Assert.That(() => t1.SubtractNotNegative(t2), Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        [TestCase("SN", "SN", true)]
        [TestCase("sn", "SN", true)]
        [TestCase(null, null, true)]
        [TestCase("SN", "XX", false)]
        [TestCase(null, "SN", false)]
        [TestCase("SN", null, false)]
        public void DeviceIdEquals(string sn1, string sn2, bool expected)
        {
            // Arrange
            var target1 = new NormalizedTimeRegisterValue(new TimeRegisterValue(sn1, DateTime.UtcNow, 2, Unit.CubicMetre), DateTime.UtcNow);
            var target2 = new NormalizedTimeRegisterValue(new TimeRegisterValue(sn2, DateTime.UtcNow, 1, Unit.CubicMetre), DateTime.UtcNow);

            // Act
            var snEquals = target1.DeviceIdEquals(target2);

            // Assert
            Assert.That(snEquals, Is.EqualTo(expected));
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            var timeRegisterValue = new TimeRegisterValue("", DateTime.MinValue.ToUniversalTime(), new UnitValue());
            var dt = new DateTime(2019, 04, 18, 15, 27, 11, DateTimeKind.Utc);

            // Act
            var target = new NormalizedTimeRegisterValue(timeRegisterValue, dt);

            // Assert
            Assert.That(target.ToString(), Is.EqualTo("[timeRegisterValue=[deviceId=, timestamp=0001-01-01T00:00:00.0000000Z, unitValue=[value=0, unit=Watt]], normalizedTimestamp=2019-04-18T15:27:11.0000000Z]"));
        }

        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var timeRegisterValue1 = new TimeRegisterValue("SN1", DateTime.UtcNow, 11, Unit.CubicMetre);
            var timeRegisterValue2 = new TimeRegisterValue("SN2", DateTime.UtcNow.AddDays(1), 22, Unit.Joule);
            var dt1 = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var dt2 = dt1.AddDays(2);
            var t1 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
            var t2 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
            var t3 = new NormalizedTimeRegisterValue(timeRegisterValue2, dt1);
            var t4 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt2);
            var t5 = (NormalizedTimeRegisterValue)null;

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t4));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t5));
            Assert.That(t5, Is.Not.EqualTo(t1));
        }

        [Test]
        public void Equeality()
        {
            // Arrange
            var timeRegisterValue1 = new TimeRegisterValue("SN1", DateTime.UtcNow, 11, Unit.CubicMetre);
            var timeRegisterValue2 = new TimeRegisterValue("SN2", DateTime.UtcNow.AddDays(1), 22, Unit.Joule);
            var dt1 = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var t1 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
            var t2 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
            var t3 = new NormalizedTimeRegisterValue(timeRegisterValue2, dt1);
            var t4 = (NormalizedTimeRegisterValue)null;

            // Act & Assert
            Assert.That(t1 == t2, Is.True);
            Assert.That(t1 == t3, Is.False);
            Assert.That(t1 == t4, Is.False);
            Assert.That(t4 == t1, Is.False);
        }

    }
}

