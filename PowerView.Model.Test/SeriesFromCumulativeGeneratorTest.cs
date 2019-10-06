using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class SeriesFromCumulativeGeneratorTest
  {
    [Test]
    public void GenerateSeriesFromCumulativeNotCumulative()
    {
      // Arrange
      var obisCode = ObisCode.ElectrActualPowerP14;
      var utcNow = DateTime.UtcNow;
      var normalizedTimeRegisterValues = new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 14, Unit.Watt)), Normalize(new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 11, Unit.Watt)) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> { { obisCode, normalizedTimeRegisterValues } });

      // Assert
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void GenerateSeriesFromCumulative()
    {
      // Arrange
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var utcNow = DateTime.UtcNow;
      var normalizedTimeRegisterValues = new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 14, Unit.WattHour)), Normalize(new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 18, Unit.WattHour)) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> { { obisCode, normalizedTimeRegisterValues } });

      // Assert
      Assert.That(result.Keys, Is.EquivalentTo(new ObisCode[] { ObisCode.ElectrActiveEnergyA14Delta, ObisCode.ElectrActiveEnergyA14Period, ObisCode.ElectrActualPowerP14Average }));
    }

    [Test]
    [TestCase("1.0.1.8.0.255", "1.67.1.7.0.255")]
    [TestCase("1.0.2.8.0.255", "1.67.2.7.0.255")]
    [TestCase("8.0.1.0.0.255", "8.67.2.0.0.255")]
    [TestCase("6.0.1.0.0.255", "6.67.8.0.0.255")]
    [TestCase("6.0.2.0.0.255", "6.67.9.0.0.255")]
    public void GenerateSeriesFromCumulativeAverages(string obisCode, string expectedAverage)
    {
      var utcNow = DateTime.UtcNow;
      var normalizedTimeRegisterValues = new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 14, Unit.WattHour)), Normalize(new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 11, Unit.WattHour)) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> { { obisCode, normalizedTimeRegisterValues } });

      // Assert
      Assert.That(result.Keys, Contains.Item((ObisCode)expectedAverage));
    }

    private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "60-minutes")
    {
      var dateTimeHelper = new DateTimeHelper(TimeZoneInfo.Local, DateTime.Today.ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
    }

  }
}
