using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LabelSeriesSetTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.UtcNow, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LabelSeriesSet<TimeRegisterValue>(DateTime.Now, DateTime.UtcNow, new LabelSeries<TimeRegisterValue>[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.Now, new LabelSeries<TimeRegisterValue>[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var start = DateTime.UtcNow - TimeSpan.FromDays(1);
      var end = DateTime.UtcNow;
      var labelSeries = new[] { new LabelSeries<TimeRegisterValue>("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()) };

      // Act
      var target = new LabelSeriesSet<TimeRegisterValue>(start, end, labelSeries);

      // Assert
      Assert.That(target.Start, Is.EqualTo(start));
      Assert.That(target.End, Is.EqualTo(end));
    }


    [Test]
    public void Enumerable()
    {
      // Arrange
      var labelSeries = new [] { 
        new LabelSeries<TimeRegisterValue>("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()), 
        new LabelSeries<TimeRegisterValue>("B", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()) 
      };

      // Act
      var target = CreateTarget(DateTime.UtcNow, DateTime.UtcNow, labelSeries);

      // Assert
      Assert.That(target, Is.EqualTo(labelSeries));
    }

    [Test]
    public void Add()
    {
      // Arrange
      var baseTime = new DateTime(2019, 7, 30, 18, 2, 12, DateTimeKind.Utc);
      var target = new LabelSeriesSet<TimeRegisterValue>(baseTime, baseTime + TimeSpan.FromMinutes(30), new LabelSeries<TimeRegisterValue>[0]);
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeRegisterValues = new[]
      {
        new TimeRegisterValue("sn1", baseTime, 11, Unit.WattHour)
      };
      var labelSeries = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

      // Act
      target.Add(labelSeries);

      // Assert
      Assert.That(target, Is.EqualTo(new[] { labelSeries }));
    }

    private static LabelSeriesSet<TimeRegisterValue> CreateTarget(DateTime start, DateTime end, IList<LabelSeries<TimeRegisterValue>> labelSeries)
    {
      return new LabelSeriesSet<TimeRegisterValue>(start, end, labelSeries);
    }

  }
}

