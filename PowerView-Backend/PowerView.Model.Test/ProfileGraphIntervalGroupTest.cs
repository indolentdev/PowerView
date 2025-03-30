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
            var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
            var start = DateTime.Today.ToUniversalTime();
            const string interval = "5-minutes";
            var profileGraphs = new List<ProfileGraph>();
            var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new TimeRegisterValueLabelSeries[0]);
            var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();

            // Act & Assert
            Assert.That(() => new ProfileGraphIntervalGroup(null, start, interval, profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, DateTime.Now, interval, profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, null, profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries), Throws.ArgumentNullException);
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, interval, null, labelSeriesSet, costBreakdownGeneratorSeries), Throws.ArgumentNullException);
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, interval, profileGraphs, null, costBreakdownGeneratorSeries), Throws.ArgumentNullException);
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, interval, profileGraphs, labelSeriesSet, null), Throws.ArgumentNullException);

            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, string.Empty, profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new ProfileGraphIntervalGroup(locationContext, start, "whatnot", profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
            var start = DateTime.Today.ToUniversalTime();
            const string label = "label";
            const string interval = "5-minutes";
            ObisCode obisCode = "1.2.3.4.5.6";
            var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
            var profileGraphs = new List<ProfileGraph> { profileGraph };
            var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } } })
      });
            var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();

            // Act
            var target = new ProfileGraphIntervalGroup(locationContext, start, interval, profileGraphs, labelSeriesSet, costBreakdownGeneratorSeries);

            // Assert
            Assert.That(target.Interval, Is.EqualTo(interval));
            Assert.That(target.ProfileGraphs, Is.EqualTo(profileGraphs));
            Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));
            Assert.That(target.CostBreakdownGeneratorSeries, Is.SameAs(costBreakdownGeneratorSeries));

            Assert.That(target.Categories, Is.Null);
            Assert.That(target.NormalizedLabelSeriesSet, Is.Null);
            Assert.That(target.NormalizedDurationLabelSeriesSet, Is.Null);
        }

    }
}
