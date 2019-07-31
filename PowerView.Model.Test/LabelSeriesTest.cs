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
