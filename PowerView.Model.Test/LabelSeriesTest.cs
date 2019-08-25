using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LabelSeriesTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      const string label = "label";
      var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue() } }
      };

      // Act & Assert
      Assert.That(() => new LabelSeries(null, timeRegisterValues), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new LabelSeries(string.Empty, timeRegisterValues), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new LabelSeries(label, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LabelSeries(label, 
        new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { "1.2.3.4.5.6", null } }), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string label = "label";
      var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue() } }
      };

      // Act
      var target = new LabelSeries(label, timeRegisterValues);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
    }

    [Test]
    public void Enumerable()
    {
      // Arrange
      const string label = "label";
      var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue() } }
      };
      var target = new LabelSeries(label, timeRegisterValues);

      // Act
      var obisCodes = target.ToList();

      // Assert
      Assert.That(obisCodes, Is.EqualTo(timeRegisterValues.Keys));
    }

    [Test]
    [TestCase("1.2.3.4.5.6", true)]
    [TestCase("6.5.4.3.2.1", false)]
    public void ContainsObisCode(string obisCode, bool expected)
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var sv1 = new TimeRegisterValue("1", dt, 21, 1, Unit.WattHour);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(5), 22, 1, Unit.WattHour);
      var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv1, sv2 } }
      };
      var target = new LabelSeries("label", timeRegisterValues);

      // Act
      var containsObisCode = target.ContainsObisCode(obisCode);

      // Assert
      Assert.That(containsObisCode, Is.EqualTo(expected));
    }

    [Test]
    public void IndexerOrderedByTimestamp()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", DateTime.UtcNow + TimeSpan.FromHours(1), 11, Unit.WattHour), new TimeRegisterValue("sn1", DateTime.UtcNow, 11, Unit.WattHour) };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act
      var timeRegisterValuesIndexer = target[obisCode];

      // Assert
      Assert.That(timeRegisterValuesIndexer, Is.EqualTo(timeRegisterValues.Reverse().ToList()));
    }

    [Test]
    public void IndexerReadOnly()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", DateTime.UtcNow, 11, Unit.WattHour) };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act & Assert
      var timeRegisterValuesIndexer = target[obisCode];

      // Assert
      Assert.That(timeRegisterValuesIndexer.IsReadOnly, Is.True);
    }

    [Test]
    public void IndexerUnknownObisCode()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

      // Act
      var timeRegisterValuesIndexer = target[obisCode];

      // Assert
      Assert.That(timeRegisterValuesIndexer, Is.Empty);
    }

    [Test]
    public void GenerateSeriesFromCumulativeNotCumulative()
    {
      // Arrange
      const string label = "label";
      var obisCode = ObisCode.ElectrActualPowerP14;
      var utcNow = DateTime.UtcNow;
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.Watt), new TimeRegisterValue("sn1", utcNow, 11, Unit.Watt) };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act
      target.GenerateSeriesFromCumulative();

      // Assert
      Assert.That(target, Is.EquivalentTo(new ObisCode[] { obisCode }));
    }

    [Test]
    public void GenerateSeriesFromCumulative()
    {
      // Arrange
      const string label = "label";
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var utcNow = DateTime.UtcNow;
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.WattHour), new TimeRegisterValue("sn1", utcNow, 11, Unit.WattHour) };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act
      target.GenerateSeriesFromCumulative();

      // Assert
      Assert.That(target, Is.EquivalentTo(new ObisCode[] { obisCode, ObisCode.ElectrActiveEnergyA14Delta, ObisCode.ElectrActiveEnergyA14Period, ObisCode.ElectrActualPowerP14Average }));
    }

    [Test]
    [TestCase("1.0.1.8.0.255", "1.67.1.7.0.255")]
    [TestCase("1.0.2.8.0.255", "1.67.2.7.0.255")]
    [TestCase("8.0.1.0.0.255", "8.67.2.0.0.255")]
    [TestCase("6.0.1.0.0.255", "6.67.8.0.0.255")]
    [TestCase("6.0.2.0.0.255", "6.67.9.0.0.255")]
    public void GenerateSeriesFromCumulativeAverages(string obisCode, string expectedAverage)
    {
      const string label = "label";
      var utcNow = DateTime.UtcNow;
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.WattHour), new TimeRegisterValue("sn1", utcNow, 11, Unit.WattHour) };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act
      target.GenerateSeriesFromCumulative();

      // Assert
      Assert.That(target, Contains.Item((ObisCode)expectedAverage));
    }

    [Test]
    public void NormalizeThrows()
    {
      // Arrange
      var target = new LabelSeries("label", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

      // Act & Assert
      Assert.That(() => target.Normalize(null), Throws.ArgumentNullException);
    }


    [Test]
    public void Normalize()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      var baseTime = new DateTime(2019, 7, 30, 18, 2, 12, DateTimeKind.Utc);
      var timeRegisterValues = new[] 
      { 
        new TimeRegisterValue("sn1", baseTime, 11, Unit.WattHour), 
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(4), 12, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(11), 13, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(16), 14, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(19), 15, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(25), 16, Unit.WattHour),
      };
      var target = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });
      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider("10-minutes");

      // Act
      var target2 = target.Normalize(timeDivider);

      // Assert
      Assert.That(target2, Is.EqualTo(target));
      Assert.That(target2[obisCode], Is.EqualTo(new TimeRegisterValue[] 
      { 
        timeRegisterValues[0].Normalize(timeDivider), timeRegisterValues[2].Normalize(timeDivider), timeRegisterValues[4].Normalize(timeDivider) 
      }));
    }

  }
}
