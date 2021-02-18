using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class ProfileGraphIntervalGroupTest
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
      Assert.That(() => new ProfileGraphIntervalGroup(null, start, interval, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, DateTime.Now, interval, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, start, null, profileGraphs, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, start, interval, null, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, start, interval, profileGraphs, null), Throws.ArgumentNullException);

      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, start, string.Empty, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new ProfileGraphIntervalGroup(timeZoneInfo, start, "whatnot", profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
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
      var target = new ProfileGraphIntervalGroup(timeZoneInfo, start, interval, profileGraphs, labelSeriesSet);

      // Assert
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.ProfileGraphs, Is.EqualTo(profileGraphs));
      Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));

      Assert.That(target.Categories, Is.Null);
      Assert.That(target.NormalizedLabelSeriesSet, Is.Null);
    }

  }
}
