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
      var profileGraphs = new List<ProfileGraph>();
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new LabelSeries<TimeRegisterValue>[0]);

      // Act & Assert
      Assert.That(() => new IntervalGroup(null, start, interval, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, DateTime.Now, interval, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, null, profileGraphs, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, interval, null, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, null), Throws.ArgumentNullException);

      Assert.That(() => new IntervalGroup(timeZoneInfo, start, string.Empty, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(timeZoneInfo, start, "whatnot", profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
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
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } } })
      });

      // Act
      var target = new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

      // Assert
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.ProfileGraphs, Is.EqualTo(profileGraphs));
      Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));

      Assert.That(target.Categories, Is.Null);
      Assert.That(target.NormalizedLabelSeriesSet, Is.Null);
    }

    [Test]
    public void Prepare_Categories_Minute()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

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
      var profileGraph = new ProfileGraph("month", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var timeZoneInfo = TimeZoneInfo.Utc;
      var start = new DateTime(2019, 3, 1, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddMonths(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

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
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { 
        new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet, Is.Not.Null);
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      var labelSeries = target.NormalizedLabelSeriesSet.First();
      Assert.That(labelSeries.Count(), Is.EqualTo(1));
      Assert.That(labelSeries[labelSeries.First()], Is.EqualTo(new[] { new NormalizedTimeRegisterValue(new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt), start) }));
    }

    [Test]
    public void Prepare_GeneratesFromCumulative()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = ObisCode.ElectrActiveEnergyA14;
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var timeZoneInfo = TimeZoneInfo.Local;
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedLabelSeriesSet.First().Count(), Is.EqualTo(4));
    }

  }
}
