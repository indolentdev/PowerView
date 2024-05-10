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
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var start = DateTime.Today.ToUniversalTime();
      const string interval = "5-minutes";
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new TimeRegisterValueLabelSeries[0]);
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();

      // Act & Assert
      Assert.That(() => new IntervalGroup(null, start, interval, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new IntervalGroup(locationContext, DateTime.Now, interval, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(locationContext, start, null, labelSeriesSet, costBreakdownGeneratorSeries), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(locationContext, start, interval, null, costBreakdownGeneratorSeries), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(locationContext, start, interval, labelSeriesSet, null), Throws.TypeOf<ArgumentNullException>());

      Assert.That(() => new IntervalGroup(locationContext, start, string.Empty, labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(locationContext, start, "whatnot", labelSeriesSet, costBreakdownGeneratorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
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
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } } })
      });
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();

      // Act
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);

      // Assert
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));
      Assert.That(target.CostBreakdownGeneratorSeries, Is.SameAs(costBreakdownGeneratorSeries));

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
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);

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
      var startLocal = new DateTime(2019, 7, 1, 00, 00, 00, DateTimeKind.Unspecified);
      var endLocal = startLocal.AddMonths(1);
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var start = locationContext.ConvertTimeToUtc(startLocal);
      var end = locationContext.ConvertTimeToUtc(endLocal);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);

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
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { 
        new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt) } } })
      });
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);
      var divider = new DateTimeHelper(locationContext, start).GetDivider(interval);

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
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries>();
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedLabelSeriesSet.First().Count(), Is.EqualTo(1));

      Assert.That(target.NormalizedDurationLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedDurationLabelSeriesSet.First().Count(), Is.EqualTo(3));
    }

    [Test]
    [TestCase("label")]
    [TestCase("otherLabel")]
    public void Prepare_GeneratesFromCostBreakdownGeneratorSeries(string genLabel)
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();

      var start = DateTime.Today.ToUniversalTime();
      var end = start.AddDays(1);
      var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(start, end, new[] {
        new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat, new[] {
        new TimeRegisterValue("EnergiDataService", start.AddHours(1), 12.34, Unit.Dkk), new TimeRegisterValue("EnergiDataService", start.AddHours(2), 23.45, Unit.Dkk) } } })
      });

      var entry = new CostBreakdownEntry(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(4444, 1, 1, 0, 0, 0, DateTimeKind.Utc), "entry", 0, 23, 12.34);
      var costBreakdown = new CostBreakdown("theTitle", Unit.Dkk, 25, new[] { entry });
      var generatorSeries = new GeneratorSeries(new SeriesName(genLabel, ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), 
        new SeriesName(label, ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), costBreakdown.Title);

      var costBreakdownGeneratorSeries = new List<CostBreakdownGeneratorSeries> { new CostBreakdownGeneratorSeries(costBreakdown, generatorSeries) };
      var target = new IntervalGroup(locationContext, start, interval, labelSeriesSet, costBreakdownGeneratorSeries);

      // Act
      target.Prepare();

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedLabelSeriesSet.First().Count(), Is.EqualTo(1));

      var labelSeries = target.NormalizedDurationLabelSeriesSet.FirstOrDefault(x => x.Label == generatorSeries.Series.Label);
      Assert.That(labelSeries, Is.Not.Null);
      Assert.That(labelSeries.ContainsObisCode(generatorSeries.Series.ObisCode));
    }

  }
}
