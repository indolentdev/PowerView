using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class IntervalGroupTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      const string interval = "5-minutes";
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new TimeRegisterValueLabelSeries[0]);

      // Act & Assert
      Assert.That(() => new IntervalGroup(null, start, interval, labelSeriesSet), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, DateTime.Now, interval, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, null, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, interval, null), Throws.ArgumentNullException);

      Assert.That(() => new IntervalGroup(timeZoneInfo, start, string.Empty, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, "whatnot", labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      const string label = "label";
      const string interval = "5-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } } })
      });

      // Act
      var target = new IntervalGroup(timeZoneInfo, start, interval, labelSeriesSet);

      // Assert
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));

      Assert.That(target.Categories, Is.Null);
      Assert.That(target.NormalizedLabelSeriesSet, Is.Null);
      Assert.That(target.NormalizedDurationLabelSeriesSet, Is.Null);
    }

    [Test]
    public void Prepare_Categories_Minute()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, labelSeriesSet);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.Categories.Count, Is.EqualTo(24));
      Assert.That(target.Categories.First(), Is.EqualTo(start));
      Assert.That(target.Categories.Last(), Is.EqualTo(end.AddHours(-1)));
    }

    [Test]
    public void Prepare_Categories_Days()
    {
      // Arrange
      const string label = "label";
      const string interval = "1-days";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeZoneInfo = TimeZoneInfo.Utc;
      var start = new DateTime(2019, 3, 1, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddMonths(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, labelSeriesSet);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.Categories.Count, Is.EqualTo(31));
      Assert.That(target.Categories.First(), Is.EqualTo(start));
      Assert.That(target.Categories.Last(), Is.EqualTo(end.AddDays(-1)));
    }

    [Test]
    public void Prepare_LabelSeriesSet()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { 
        new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, labelSeriesSet);
      var divider = new DateTimeHelper(timeZoneInfo, start).GetDivider(interval);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet, Is.Not.Null);
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      var labelSeriesTime = target.NormalizedLabelSeriesSet.First();
      Assert.That(labelSeriesTime.Count(), Is.EqualTo(1));
      Assert.That(labelSeriesTime[labelSeriesTime.First()], Is.EqualTo(new[] { new NormalizedTimeRegisterValue(new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt), start) }));

      Assert.That(target.NormalizedDurationLabelSeriesSet, Is.Not.Null);
      Assert.That(target.NormalizedDurationLabelSeriesSet.Count(), Is.EqualTo(1));
      var labelSeriesDuration = target.NormalizedDurationLabelSeriesSet.First();
      Assert.That(labelSeriesDuration.Count(), Is.EqualTo(1));
      Assert.That(labelSeriesDuration[labelSeriesDuration.First()], Is.EqualTo(new[] { 
        new NormalizedDurationRegisterValue(start.AddMinutes(2), start.AddMinutes(2), divider(start.AddMinutes(2)), divider(start.AddMinutes(2)), new UnitValue(1234, Unit.Watt), "SN1") }));
    }

    [Test]
    public void Prepare_GeneratesFromCumulative()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = ObisCode.ElectrActiveEnergyA14;
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, labelSeriesSet);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedLabelSeriesSet.First().Count(), Is.EqualTo(1));

      Assert.That(target.NormalizedDurationLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedDurationLabelSeriesSet.First().Count(), Is.EqualTo(3));
    }

  }
}
