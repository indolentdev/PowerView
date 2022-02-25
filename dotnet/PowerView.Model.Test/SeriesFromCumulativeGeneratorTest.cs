using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class SeriesFromCumulativeGeneratorTest
  {
    [Test]
    public void GenerateNotCumulative()
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
    public void GeneratePeriodsAndDeltas()
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
    [TestCase("9.0.1.0.0.255", "9.67.2.0.0.255")]
    [TestCase("6.0.1.0.0.255", "6.67.8.0.0.255")]
    [TestCase("6.0.2.0.0.255", "6.67.9.0.0.255")]
    public void GenerateAverages(string obisCode, string expectedAverage)
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var normalizedTimeRegisterValues = new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 14, Unit.WattHour)), Normalize(new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 11, Unit.WattHour)) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> { { obisCode, normalizedTimeRegisterValues } });

      // Assert
      Assert.That(result.Keys, Contains.Item((ObisCode)expectedAverage));
    }

    [Test]
    public void GenerateDiffs()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var oneHour = TimeSpan.FromHours(1);
      var dict = new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>>
      {
        { ObisCode.ElectrActiveEnergyA14, new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 14, Unit.WattHour)),
            Normalize(new TimeRegisterValue("sn1", utcNow + oneHour, 20, Unit.WattHour)), Normalize(new TimeRegisterValue("sn1", utcNow + oneHour + oneHour, 23, Unit.WattHour)) } },
        { ObisCode.ElectrActiveEnergyA23, new[] { Normalize(new TimeRegisterValue("sn1", utcNow, 2, Unit.WattHour)),
            Normalize(new TimeRegisterValue("sn1", utcNow + oneHour, 4, Unit.WattHour)), Normalize(new TimeRegisterValue("sn1", utcNow + oneHour+ oneHour, 5, Unit.WattHour)) } }
      };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(dict);

      // Assert
      Assert.That(result.Keys, Contains.Item(ObisCode.ElectrActiveEnergyA14NetDelta));
      Assert.That(result.Keys, Contains.Item(ObisCode.ElectrActiveEnergyA23NetDelta));
    }

    private static NormalizedTimeRegisterValue Normalize(TimeRegisterValue timeRegisterValue, string interval = "60-minutes")
    {
      var dateTimeHelper = new DateTimeHelper(TimeZoneInfo.Local, DateTime.Today.ToUniversalTime());
      var timeDivider = dateTimeHelper.GetDivider(interval);
      return new NormalizedTimeRegisterValue(timeRegisterValue, timeDivider(timeRegisterValue.Timestamp));
    }

  }
}
