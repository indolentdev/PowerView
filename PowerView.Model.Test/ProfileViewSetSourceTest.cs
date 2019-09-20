using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class ProfileViewSetSourceTest
  {
    private const Unit unit = Unit.WattHour;

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var intervalGroup = GetIntervalGroup();
      var profileGraph = intervalGroup.ProfileGraphs.First();

      // Act & Assert
      Assert.That(() => new ProfileViewSetSource(null, new[] { intervalGroup }), Throws.ArgumentNullException);
      Assert.That(() => new ProfileViewSetSource(new[] { profileGraph }, null), Throws.ArgumentNullException);

      Assert.That(() => new ProfileViewSetSource(new[] { profileGraph, profileGraph }, new[] { intervalGroup }), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new ProfileViewSetSource(new[] { GetProfileGraph() }, new[] { intervalGroup }), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetProfileViewSet_SourceLabelSeriesEmpty()
    {
      // Arrange
      var profileGraph = GetProfileGraph();
      var intervalGroup = GetIntervalGroup(fromFirstCount: 0, profileGraphs: new ProfileGraph[] { profileGraph });
      var target = CreateTarget(profileGraph, intervalGroup);

      // Act
      var profileViewSet = target.GetProfileViewSet();

      // Assert
      Assert.That(profileViewSet.SerieSets, Is.Empty);
      Assert.That(profileViewSet.PeriodTotals, Is.Empty);
    }

    [Test]
    public void GetProfileViewSet_SourceLabelSeriesCategoryComplete()
    {
      // Arrange
      var intervalGroup = GetIntervalGroup(baseValue: 2000);
      var profileGraph = intervalGroup.ProfileGraphs.First();
      var target = CreateTarget(profileGraph, intervalGroup);

      // Act
      var profileViewSet = target.GetProfileViewSet();

      // Assert
      Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(1));
      Assert.That(profileViewSet.SerieSets.First().Title, Is.EqualTo(profileGraph.Title));
      Assert.That(profileViewSet.SerieSets.First().Categories, Is.EqualTo(intervalGroup.Categories));
      Assert.That(profileViewSet.SerieSets.First().Series.Count, Is.EqualTo(1));
      Assert.That(profileViewSet.SerieSets.First().Series.First().SeriesName, Is.EqualTo(profileGraph.SerieNames.First()));
      Assert.That(profileViewSet.SerieSets.First().Series.First().Unit, Is.EqualTo(unit));
      var expectedValues = Enumerable.Range(2001, 24).Select(x => (double?)x).ToArray();
      Assert.That(profileViewSet.SerieSets.First().Series.First().Values, Is.EqualTo(expectedValues));
      Assert.That(profileViewSet.PeriodTotals, Is.Empty);
    }

    [Test]
    public void GetProfileViewSet_SourceLabelSeriesPartiallyCategoryComplete()
    {
      // Arrange
      var intervalGroup = GetIntervalGroup(fromFirstCount:13, baseValue: 2000);
      var profileGraph = intervalGroup.ProfileGraphs.First();
      var target = CreateTarget(profileGraph, intervalGroup);

      // Act
      var profileViewSet = target.GetProfileViewSet();

      // Assert
      Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(1));
      Assert.That(profileViewSet.SerieSets.First().Series.Count, Is.EqualTo(1));
      var expectedValues = Enumerable.Range(2001, 12).Select(x => (double?)x).Concat(Enumerable.Repeat(default(double?), 12)).ToArray();
      Assert.That(profileViewSet.SerieSets.First().Series.First().Values, Is.EqualTo(expectedValues));
      Assert.That(profileViewSet.PeriodTotals, Is.Empty);
    }

    [Test]
    public void GetProfileViewSet_SourceLabelSeriesWithPeriodGoesToTotal()
    {
      // Arrange
      var serieName = new SeriesName("TheLabel", ObisCode.ElectrActiveEnergyA14Period);
      var profileGraph = GetProfileGraph(seriesNames: new [] { serieName });
      var intervalGroup = GetIntervalGroup(baseValue: 2000, profileGraphs: new ProfileGraph[] { profileGraph });
      var target = CreateTarget(profileGraph, intervalGroup);

      // Act
      var profileViewSet = target.GetProfileViewSet();

      // Assert
      Assert.That(profileViewSet.PeriodTotals.Count, Is.EqualTo(1));
      Assert.That(profileViewSet.PeriodTotals.First().SerieName, Is.EqualTo(serieName));
      Assert.That(profileViewSet.PeriodTotals.First().UnitValue, Is.EqualTo(new UnitValue(2024, unit)));
    }

    [Test]
    public void T()
    {
      // Arrange
      var dts = new[]
      {
      "2018-08-31T22:00Z",
      "2018-09-30T22:00Z",
      "2018-10-31T22:00Z",
      "2018-11-30T22:00Z",
      "2018-12-31T22:00Z",
      "2019-01-31T22:00Z",
      "2019-02-28T22:00Z",
      "2019-03-31T22:00Z",
      "2019-04-30T22:00Z",
      "2019-05-31T22:00Z",
      "2019-06-30T22:00Z",
      "2019-07-31T22:00Z"
      };
      var eh = dts.Select(x => DateTime.Parse(x, System.Globalization.CultureInfo.InvariantCulture, 
      System.Globalization.DateTimeStyles.RoundtripKind)).ToList();

      // Act
      var xx = ProfileViewSetSource.XCategories(TimeZoneInfo.Local, eh).ToList();

      // Assert
    }


    private static ProfileViewSetSource CreateTarget(ProfileGraph profileGraph, IntervalGroup intervalGroup)
    {
      return CreateTarget(new ProfileGraph[] { profileGraph }, new IntervalGroup[] { intervalGroup });
    }

    private static ProfileViewSetSource CreateTarget(IEnumerable<ProfileGraph> profileGraphs, IList<IntervalGroup> intervalGroups)
    {
      return new ProfileViewSetSource(profileGraphs, intervalGroups);
    }

    private static ProfileGraph GetProfileGraph(string title = "TheTitle", string interval = "60-minutes", params SeriesName[] seriesNames)
    {
      if (seriesNames.Length == 0)
      {
        seriesNames = new[] { new SeriesName("TheLabel", "1.2.3.4.5.6") }.ToArray();
      }
      return new ProfileGraph("day", "ThePage", title, interval, 1, seriesNames);
    }

    private static IntervalGroup GetIntervalGroup(DateTime? firstTimestamp = null, int? fromFirstCount = null, DateTime? start = null, int? fromStartCount = null, TimeSpan? interval = null, int? baseValue = null, params ProfileGraph[] profileGraphs)
    {
      if (profileGraphs.Length == 0)
      {
        profileGraphs = new[] { GetProfileGraph() };
      }
      if (start == null) start = new DateTime(2019, 8, 26, 0, 0, 0, DateTimeKind.Utc);
      var intervalGroup = new IntervalGroup(start.Value, profileGraphs.First().Interval, profileGraphs, GetLabelSeriesSet(firstTimestamp, fromFirstCount, start, fromStartCount, interval, baseValue, profileGraphs));
      intervalGroup.Prepare(new LabelObisCodeTemplate[0]);
      return intervalGroup;
    }

    private static LabelSeriesSet<TimeRegisterValue> GetLabelSeriesSet(DateTime? firstTimestamp, int? fromFirstCount, DateTime? start, int? fromStartCount, TimeSpan? interval, int? baseValue, IEnumerable<ProfileGraph> profileGraphs)
    {
      if (firstTimestamp == null) firstTimestamp = new DateTime(2019, 8, 25, 23, 5, 0, DateTimeKind.Utc);
      if (fromFirstCount == null) fromFirstCount = 25;
      if (start == null) start = new DateTime(2019, 8, 26, 0, 0, 0, DateTimeKind.Utc);
      if (fromStartCount == null) fromStartCount = 24;
      if (interval == null) interval = TimeSpan.FromMinutes(60);
      if (baseValue == null) baseValue = 1200;
      if (!profileGraphs.Any())
      {
        profileGraphs = new[] { GetProfileGraph() };
      }

      var labelSeries = profileGraphs
        .SelectMany(x => x.SerieNames)
        .Distinct()
        .GroupBy(x => x.Label, x => x.ObisCode)
        .Select(x => new LabelSeries<TimeRegisterValue>(x.Key, GetTimeRegisterValues(x, firstTimestamp.Value, interval.Value, fromFirstCount.Value, baseValue.Value)))
        .Where(x => x.Any())
        .ToList();

      return new LabelSeriesSet<TimeRegisterValue>(start.Value, start.Value + TimeSpan.FromMilliseconds(interval.Value.TotalMilliseconds * fromStartCount.Value), labelSeries);
    }

    private static IDictionary<ObisCode, IEnumerable<TimeRegisterValue>> GetTimeRegisterValues(IEnumerable<ObisCode> obisCodes, DateTime firstTimestamp, TimeSpan interval, int count, int baseValue)
    {
      return obisCodes
        .Select(x => new { ObisCode = x, Values = Enumerable.Range(0, count).Select(i => new TimeRegisterValue("SN1", firstTimestamp + TimeSpan.FromMilliseconds(interval.TotalMilliseconds * i), baseValue + i, unit)).ToList() })
        .Where(x => x.Values.Any())
        .ToDictionary(x => x.ObisCode, x => (IEnumerable<TimeRegisterValue>)x.Values);
    }

  }
}
