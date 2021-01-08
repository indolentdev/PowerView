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
      var dict = new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>>
      {
        { "1.2.3.4.5.6", new List<NormalizedTimeRegisterValue>() },
        { "6.5.4.3.2.1", new List<NormalizedTimeRegisterValue>() }
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
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt, 210, 1, unit) };
      var values = GetDictionary(v11);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, "6.6.6.6.6.6");

      // Act
      target.CalculateNext(normalizedTimestamp, values);
      var generatedValues = target.GetGenerated();

      // Assert
      Assert.That(generatedValues, Is.Empty);
    }

    [Test]
    public void DifferentDeviceIds()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("ID-1", dt, 210, 1, unit) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("ID-2", dt, 210, 1, unit) };
      var values = GetDictionary(v11, v21);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(normalizedTimestamp, values);
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
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt, 210, 1, Unit.WattHour) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt, 210, 1, Unit.Joule) };
      var values = GetDictionary(v11, v21);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(normalizedTimestamp, values);
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
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt, minutend, 0, unit) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt, substrahend, 0, unit) };
      var values = GetDictionary(v11, v21);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(normalizedTimestamp, values);
      var generatedValues = target.GetGenerated();

      // Assert
      var expectedValue = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", dt, difference, unit), normalizedTimestamp);
      Assert.That(generatedValues, Is.EqualTo(new [] { expectedValue }));
    }

    [Test]
    public void DifferenceSequence()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt, 123, 0, unit) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt, 100, 0, unit) };
      var v12 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt + TimeSpan.FromHours(1), 234, 0, unit) };
      var v22 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt + TimeSpan.FromHours(1), 100, 0, unit) };
      var v13 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt + TimeSpan.FromHours(2), 12, 0, unit) };
      var v23 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt + TimeSpan.FromHours(2), 13, 0, unit) };
      var dtNormalized = NormalizeTimestamp(dt, "60-minutes");
      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(dtNormalized, GetDictionary(v11, v21));
      target.CalculateNext(dtNormalized + TimeSpan.FromHours(1), GetDictionary(v12, v22));
      target.CalculateNext(dtNormalized + TimeSpan.FromHours(2), GetDictionary(v13, v23));
      var generatedValues = target.GetGenerated();

      // Assert
      var expectedValue1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", dt, 23, unit), dtNormalized);
      var expectedValue2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", dt + TimeSpan.FromHours(1), 134, unit), dtNormalized + TimeSpan.FromHours(1));
      var expectedValue3 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", dt + TimeSpan.FromHours(2), 0, unit), dtNormalized + TimeSpan.FromHours(2));
      Assert.That(generatedValues, Is.EqualTo(new[] { expectedValue1, expectedValue2, expectedValue3 }));
    }

    [Test]
    public void MeanTimestampMinutendTimestampLarger()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt + TimeSpan.FromMinutes(2), 210, 1, unit) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt, 210, 1, unit) };
      var values = GetDictionary(v11, v21);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(normalizedTimestamp, values);
      var generatedValues = target.GetGenerated();

      // Assert
      Assert.That(generatedValues.Select(x => x.TimeRegisterValue.Timestamp).ToList(), Is.EqualTo(new[] { dt + TimeSpan.FromMinutes(1) }));
    }

    [Test]
    public void MeanTimestampSubstrahendTimestampLarger()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 01, 00, DateTimeKind.Local).ToUniversalTime();
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      var v11 = new { ObisCode = obisCode1, Value = new TimeRegisterValue("1", dt, 210, 1, unit) };
      var v21 = new { ObisCode = obisCode2, Value = new TimeRegisterValue("1", dt + TimeSpan.FromMinutes(2), 210, 1, unit) };
      var values = GetDictionary(v11, v21);
      var normalizedTimestamp = NormalizeTimestamp(dt, "5-minutes");

      var target = new DiffByTimeSeriesGenerator(obisCode1, obisCode2);

      // Act
      target.CalculateNext(normalizedTimestamp, values);
      var generatedValues = target.GetGenerated();

      // Assert
      Assert.That(generatedValues.Select(x => x.TimeRegisterValue.Timestamp).ToList(), Is.EqualTo(new[] { dt + TimeSpan.FromMinutes(1) }));
    }

    private static IDictionary<ObisCode, TimeRegisterValue> GetDictionary(params dynamic[] values)
    {
      var dict = values
        .Select(x => new { ObisCode = (ObisCode)x.ObisCode, Value = (TimeRegisterValue)x.Value })
        .GroupBy(x => x.ObisCode)
        .ToDictionary(x => x.Key)
        .ToDictionary(x => x.Key, xx => xx.Value.Single().Value);

      return dict;
    }


    private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "5-minutes")
    {
      var dateTimeHelper = new DateTimeHelper(TimeZoneInfo.Local, new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp, string interval = "5-minutes")
    {
      var dateTimeHelper = new DateTimeHelper(TimeZoneInfo.Local, new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return timeDivider(timestamp);
    }


  }
}
