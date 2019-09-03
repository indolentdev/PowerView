﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class IntervalGroupTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var start = DateTime.UtcNow;
      const string interval = "5-minutes";
      var profileGraphs = new List<ProfileGraph>();
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new LabelSeries<TimeRegisterValue>[0]);

      // Act & Assert
      Assert.That(() => new IntervalGroup(DateTime.Now, interval, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(start, null, profileGraphs, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(start, interval, null, labelSeriesSet), Throws.ArgumentNullException);
      Assert.That(() => new IntervalGroup(start, interval, profileGraphs, null), Throws.ArgumentNullException);

      Assert.That(() => new IntervalGroup(start, string.Empty, profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalGroup(start, "whatnot", profileGraphs, labelSeriesSet), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var start = DateTime.UtcNow;
      const string label = "label";
      const string interval = "5-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(1), new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { new TimeRegisterValue() } } })
      });

      // Act
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);

      // Assert
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.ProfileGraphs, Is.EqualTo(profileGraphs));
      Assert.That(target.LabelSeriesSet, Is.SameAs(labelSeriesSet));

      Assert.That(target.Categories, Is.Null);
      Assert.That(target.NormalizedCategories, Is.Null);
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
      var start = new DateTime(2019, 4, 11, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);
      var labelObisCodeTemplate = new LabelObisCodeTemplate("newTemplate", new ObisCodeTemplate[0]);
      var labelObisCodeTemplates = new[] { labelObisCodeTemplate };

      // Act
      target.Prepare(labelObisCodeTemplates);

      // Assert
      Assert.That(target.Categories.Count, Is.EqualTo(24));
      Assert.That(target.Categories.First(), Is.EqualTo(start));
      Assert.That(target.Categories.Last(), Is.EqualTo(end.AddHours(-1)));
      Assert.That(target.NormalizedCategories.Count, Is.EqualTo(24));
      Assert.That(target.NormalizedCategories.First(), Is.EqualTo(start));
      Assert.That(target.NormalizedCategories.Last(), Is.EqualTo(end.AddHours(-1)));
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
      var start = new DateTime(2019, 3, 1, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddMonths(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);
      var labelObisCodeTemplate = new LabelObisCodeTemplate("newTemplate", new ObisCodeTemplate[0]);
      var labelObisCodeTemplates = new[] { labelObisCodeTemplate };

      // Act
      target.Prepare(labelObisCodeTemplates);

      // Assert
      Assert.That(target.Categories.Count, Is.EqualTo(31));
      Assert.That(target.Categories.First(), Is.EqualTo(start));
      Assert.That(target.Categories.Last(), Is.EqualTo(end.AddDays(-1)));
      Assert.That(target.NormalizedCategories.Count, Is.EqualTo(31));
      Assert.That(target.NormalizedCategories.First(), Is.EqualTo(start));
      Assert.That(target.NormalizedCategories.Last(), Is.EqualTo(end.AddDays(-1)));
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
      var start = new DateTime(2019, 4, 11, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] { 
        new TimeRegisterValue("SN1", start.AddMinutes(2), 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);
      var labelObisCodeTemplate = new LabelObisCodeTemplate("newTemplate", new ObisCodeTemplate[0]);
      var labelObisCodeTemplates = new[] { labelObisCodeTemplate };

      // Act
      target.Prepare(labelObisCodeTemplates);

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
      var start = new DateTime(2019, 4, 11, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);
      var labelObisCodeTemplate = new LabelObisCodeTemplate("newTemplate", new ObisCodeTemplate[0]);
      var labelObisCodeTemplates = new[] { labelObisCodeTemplate };

      // Act
      target.Prepare(labelObisCodeTemplates);

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(1));
      Assert.That(target.NormalizedLabelSeriesSet.First().Count(), Is.EqualTo(4));
    }

    [Test]
    public void Prepare_GeneratesFromTemplates()
    {
      // Arrange
      const string label = "label";
      const string interval = "60-minutes";
      ObisCode obisCode = "1.2.3.4.5.6";
      var profileGraph = new ProfileGraph("day", "The Page", "The Title", interval, 1, new[] { new SeriesName(label, obisCode) });
      var profileGraphs = new List<ProfileGraph> { profileGraph };
      var start = new DateTime(2019, 4, 11, 00, 00, 00, DateTimeKind.Utc);
      var end = start.AddDays(1);
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(start, end, new[] {
        new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, new[] {
        new TimeRegisterValue("SN1", start, 1234, Unit.Watt) } } })
      });
      var target = new IntervalGroup(start, interval, profileGraphs, labelSeriesSet);
      var obisCodeTemplate = new ObisCodeTemplate("6.5.4.3.2.1", new RegisterTemplateExpression(label + ":" + obisCode));
      var labelObisCodeTemplate = new LabelObisCodeTemplate("new-label", new[] { obisCodeTemplate });
      var labelObisCodeTemplates = new[] { labelObisCodeTemplate };

      // Act
      target.Prepare(labelObisCodeTemplates);

      // Assert
      Assert.That(target.NormalizedLabelSeriesSet.Count(), Is.EqualTo(2));
    }

  }
}
