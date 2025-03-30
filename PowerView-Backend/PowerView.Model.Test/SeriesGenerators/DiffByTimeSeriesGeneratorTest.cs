using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model.Test.SeriesGenerators
{
    [TestFixture]
    public class DiffByTimeSeriesGeneratorTest
    {
        [Test]
        [TestCase("1.2.3.4.5.6", "6.5.4.3.2.1", true)]
        [TestCase("6.5.4.3.2.1", "1.2.3.4.5.6", true)]
        [TestCase("1.2.3.4.5.6", "7.7.7.7.7.7", false)]
        [TestCase("7.7.7.7.7.7", "1.2.3.4.5.6", false)]
        public void IsSatisfiedBy(string minutendObisCode, string substrahendObisCoe, bool expectedResult)
        {
            // Arrange
            var dict = new Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>>
      {
        { "1.2.3.4.5.6", new List<NormalizedDurationRegisterValue>() },
        { "6.5.4.3.2.1", new List<NormalizedDurationRegisterValue>() }
      };

            var target = new DiffByTimeSeriesGenerator(minutendObisCode, substrahendObisCoe);

            // Act & Assert
            Assert.That(target.IsSatisfiedBy(dict), Is.EqualTo(expectedResult));
        }

        [Test]
        public void JustOneOfTheTwoNeededObisCode()
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, 12, unit) };
            var values = GetDictionary(v11);

            var target = new DiffByTimeSeriesGenerator(obisCode1, "6.6.6.6.6.6");

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            Assert.That(generatedValues, Is.Empty);
        }

        [Test]
        public void NormalizedTimestampsDoNotMatch()
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, 12, unit) };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt.AddMinutes(10), 12, unit) };
            var values = GetDictionary(v11, v21);

            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            Assert.That(generatedValues, Is.Empty);
        }

        [Test]
        public void DifferentUnits()
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, 12, Unit.WattHour) };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt, 12, Unit.Joule) };
            var values = GetDictionary(v11, v21);

            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            Assert.That(generatedValues, Is.Empty);
        }

        [Test]
        [TestCase(345, 123, 222)]
        [TestCase(123, 345, 0)]
        public void Difference(int minutend, int substrahend, int difference)
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, minutend, unit) };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt, substrahend, unit) };
            var values = GetDictionary(v11, v21);

            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            var expectedValue = Value(dt, difference, unit);
            Assert.That(generatedValues, Is.EqualTo(new[] { expectedValue }));
        }

        [Test]
        public void ValuesWithDifferentDeviceIds()
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, 22, unit, deviceIds: new[] { "DevID1" }) };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt, 10, unit, deviceIds: new[] { "DevID2" }) };
            var values = GetDictionary(v11, v21);

            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            var firstItemDeviceIds = generatedValues.Select(x => x.DeviceIds).First();
            Assert.That(firstItemDeviceIds, Is.EqualTo(new string[] { "DevID1", "DevID2" }));
        }

        [Test]
        public void DifferenceSequence()
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt, 123, unit, "60-minutes") };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt, 100, unit, "60-minutes") };
            var v12 = new { ObisCode = obisCode1, Value = Value(dt + TimeSpan.FromHours(1), 234, unit, "60-minutes") };
            var v22 = new { ObisCode = obisCode2, Value = Value(dt + TimeSpan.FromHours(1), 100, unit, "60-minutes") };
            var v13 = new { ObisCode = obisCode1, Value = Value(dt + TimeSpan.FromHours(2), 12, unit, "60-minutes") };
            var v23 = new { ObisCode = obisCode2, Value = Value(dt + TimeSpan.FromHours(2), 13, unit, "60-minutes") };
            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(GetDictionary(v11, v21));
            target.CalculateNext(GetDictionary(v12, v22));
            target.CalculateNext(GetDictionary(v13, v23));
            var generatedValues = target.GetGenerated();

            // Assert
            var expectedValue1 = new NormalizedDurationRegisterValue(v11.Value.Start, v11.Value.End, v11.Value.NormalizedStart, v11.Value.NormalizedEnd, new UnitValue(23, v11.Value.UnitValue.Unit), v11.Value.DeviceIds.ToArray());
            var expectedValue2 = new NormalizedDurationRegisterValue(v12.Value.Start, v12.Value.End, v12.Value.NormalizedStart, v12.Value.NormalizedEnd, new UnitValue(134, v12.Value.UnitValue.Unit), v12.Value.DeviceIds.ToArray());
            var expectedValue3 = new NormalizedDurationRegisterValue(v13.Value.Start, v13.Value.End, v13.Value.NormalizedStart, v13.Value.NormalizedEnd, new UnitValue(0, v13.Value.UnitValue.Unit), v13.Value.DeviceIds.ToArray());
            Assert.That(generatedValues, Is.EqualTo(new[] { expectedValue1, expectedValue2, expectedValue3 }));
        }

        [Test]
        public void TimestampMinutendTimestampLarger()
        {
            // Arrange
            var unit = Unit.WattHour;
            var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            var v11 = new { ObisCode = obisCode1, Value = Value(dt + TimeSpan.FromMinutes(2), 210, unit) };
            var v21 = new { ObisCode = obisCode2, Value = Value(dt, 200, unit) };
            var values = GetDictionary(v11, v21);

            var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

            // Act
            target.CalculateNext(values);
            var generatedValues = target.GetGenerated();

            // Assert
            Assert.That(generatedValues.Select(x => x.Start).ToList(), Is.EqualTo(new[] { dt }));
            Assert.That(generatedValues.Select(x => x.End).ToList(), Is.EqualTo(new[] { dt + TimeSpan.FromMinutes(2 + 5) }));
        }

        private static IDictionary<ObisCode, NormalizedDurationRegisterValue> GetDictionary(params dynamic[] values)
        {
            var dict = values
              .Select(x => new { ObisCode = (ObisCode)x.ObisCode, Value = (NormalizedDurationRegisterValue)x.Value })
              .GroupBy(x => x.ObisCode)
              .ToDictionary(x => x.Key)
              .ToDictionary(x => x.Key, xx => xx.Value.Single().Value);

            return dict;
        }


        private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "5-minutes")
        {
            var dateTimeHelper = GetDateTimeHelper();
            var timeDivider = dateTimeHelper.GetDivider(interval);
            return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
        }

        private static DateTime NormalizeTimestamp(DateTime timestamp, string interval = "5-minutes")
        {
            var dateTimeHelper = GetDateTimeHelper();
            var timeDivider = dateTimeHelper.GetDivider(interval);
            return timeDivider(timestamp);
        }

        private static NormalizedDurationRegisterValue Value(DateTime timestamp, double value, Unit unit, string interval = "5-minutes", params string[] deviceIds)
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
