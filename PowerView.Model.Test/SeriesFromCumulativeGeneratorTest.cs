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
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.Watt), new TimeRegisterValue("sn1", utcNow, 11, Unit.Watt) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Assert
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void GenerateSeriesFromCumulative()
    {
      // Arrange
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var utcNow = DateTime.UtcNow;
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.WattHour), new TimeRegisterValue("sn1", utcNow, 18, Unit.WattHour) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

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
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.WattHour), new TimeRegisterValue("sn1", utcNow, 11, Unit.WattHour) };
      var target = new SeriesFromCumulativeGenerator();

      // Act
      var result = target.Generate(new Dictionary<ObisCode, IList<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Assert
      Assert.That(result.Keys, Contains.Item((ObisCode)expectedAverage));
    }

  }
}
