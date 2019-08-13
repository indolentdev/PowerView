using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using SV=PowerView.Model.TimeRegisterValue;
using PowerView.Model.Expression;
using Moq;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LabelSeriesSetTest
  {
    private const Unit w = Unit.Watt;
    private const Unit wh = Unit.WattHour;
    private const Unit m3 = Unit.CubicMetre;
    private const Unit m3h = Unit.CubicMetrePrHour;
    private const Unit c = Unit.DegreeCelsius;
    private const Unit pct = Unit.Percentage;

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new LabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LabelSeriesSet(DateTime.Now, DateTime.UtcNow, new LabelSeries[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new LabelSeriesSet(DateTime.UtcNow, DateTime.Now, new LabelSeries[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var start = DateTime.UtcNow - TimeSpan.FromDays(1);
      var end = DateTime.UtcNow;
      var labelSeries = new[] { new LabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()) };

      // Act
      var target = new LabelSeriesSet(start, end, labelSeries);

      // Assert
      Assert.That(target.Start, Is.EqualTo(start));
      Assert.That(target.End, Is.EqualTo(end));
    }


    [Test]
    public void Enumerable()
    {
      // Arrange
      var labelSeries = new [] { 
        new LabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()), 
        new LabelSeries("B", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()) 
      };

      // Act
      var target = CreateTarget(DateTime.UtcNow, DateTime.UtcNow, labelSeries);

      // Assert
      Assert.That(target, Is.EqualTo(labelSeries));
    }

    [Test]
    public void GenerateSeriesFromCumulative()
    {
      // Arrange
      var obisCode = ObisCode.ElectrActiveEnergyA14;
      var utcNow = DateTime.UtcNow;
      var timeRegisterValues = new[] { new TimeRegisterValue("sn1", utcNow + TimeSpan.FromHours(1), 14, Unit.WattHour), new TimeRegisterValue("sn1", utcNow, 11, Unit.WattHour) };
      var labelSeries1 = new LabelSeries("label1", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });
      var labelSeries2 = new LabelSeries("label2", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });
      var target = CreateTarget(utcNow, utcNow, new[] { labelSeries1, labelSeries2 });

      // Act
      target.GenerateSeriesFromCumulative();

      // Assert
      Assert.That(target.Count(), Is.EqualTo(2));
      Assert.That(target.First().Count(), Is.GreaterThan(1));
      Assert.That(target.Last().Count(), Is.GreaterThan(1));
    }

    [Test]
    public void NormalizeThrows()
    {
      // Arrange
      var labelSeries = new[] {
        new LabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()),
      };
      var target = CreateTarget(DateTime.UtcNow, DateTime.UtcNow, labelSeries);

      // Act
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
      var labelSeries = new LabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });
      var target = new LabelSeriesSet(baseTime, baseTime + TimeSpan.FromMinutes(30), new[] { labelSeries });
      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider("10-minutes");

      // Act
      var target2 = target.Normalize(timeDivider);

      // Assert
      Assert.That(target2.Start, Is.EqualTo(target.Start));
      Assert.That(target2.End, Is.EqualTo(target.End));
      Assert.That(target2.Count(), Is.EqualTo(target.Count()));
      Assert.That(target2.First()[obisCode], Is.EqualTo(new TimeRegisterValue[]
      {
        timeRegisterValues[0].Normalize(timeDivider), timeRegisterValues[2].Normalize(timeDivider), timeRegisterValues[4].Normalize(timeDivider)
      }));
    }

    /*
        [Test]
        public void GetProfileViewSetEmptyLabelProfileSet()
        {
          // Arrange
          ProfileGraph profileGraph = new ProfileGraph("day", "MyPage", "MyTitle", "5-minutes", 0, new [] { new SeriesName("TheLabel", ObisCode.ElectrActiveEnergyA14 ) });
          var profileGraphs = new[] { profileGraph };
          var labelProfile = new LabelProfile("TheLabel", DateTime.UtcNow, new Dictionary<ObisCode, ICollection<SV>>());
          var target = CreateTarget(DateTime.UtcNow, new [] { labelProfile });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.SerieSets, Is.Empty);
          Assert.That(profileViewSet.PeriodTotals, Is.Empty);
        }

        [Test]
        public void GetProfileViewSetOneSerie()
        {
          // Arrange
          var serieName = new SeriesName("TheLabel", ObisCode.ElectrActualPowerP14);
          ProfileGraph profileGraph = new ProfileGraph("day", "MyPage", "MyTitle", "5-minutes", 0, new[] { serieName });
          var profileGraphs = new[] { profileGraph };
          var ts1 = new DateTime(2000, 1, 1, 1, 0, 0, DateTimeKind.Utc);
          var ts2 = ts1 + TimeSpan.FromMinutes(5);
          var labelProfile = new LabelProfile(serieName.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
              { serieName.ObisCode, new [] { new SV("1", ts1, 4, w), new SV("1", ts2, 5, w) }}
          });
          var target = CreateTarget(DateTime.UtcNow, new[] { labelProfile });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(1));
          Assert.That(profileViewSet.SerieSets.First().Categories, Is.EqualTo(new []{ts1, ts2}));
          Assert.That(profileViewSet.SerieSets.First().Series.Count, Is.EqualTo(1));
          Assert.That(profileViewSet.SerieSets.First().Series.First().SeriesName, Is.EqualTo(serieName));
          Assert.That(profileViewSet.SerieSets.First().Series.First().Unit, Is.EqualTo(w));
          Assert.That(profileViewSet.SerieSets.First().Series.First().Values, Is.EqualTo(new double[] {4, 5}));
        }

        [Test]
        public void GetProfileViewSetTwoSeries()
        {
          // Arrange
          var serieName1 = new SeriesName("TheLabel", ObisCode.ElectrActualPowerP14);
          var serieName2 = new SeriesName("TheLabel", ObisCode.ElectrActualPowerP23);
          ProfileGraph profileGraph = new ProfileGraph("day", "MyPage", "MyTitle", "5-minutes", 0, new[] { serieName1, serieName2 });
          var profileGraphs = new[] { profileGraph };
          var ts1 = new DateTime(2000, 1, 2, 3, 5, 0, DateTimeKind.Utc);
          var ts2 = new DateTime(2000, 1, 2, 3, 10, 0, DateTimeKind.Utc);
          var labelProfile = new LabelProfile(serieName1.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName1.ObisCode, new [] { new SV("1", ts1, 4, w), new SV("1", ts2, 5, w) }},
            { serieName2.ObisCode, new [] { new SV("1", ts1, 6, w), new SV("1", ts2, 7, w) }}
          });
          var target = CreateTarget(DateTime.UtcNow, new[] { labelProfile });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(1));
          Assert.That(profileViewSet.SerieSets.First().Categories, Is.EqualTo(new[] { ts1, ts2 }));
          Assert.That(profileViewSet.SerieSets.First().Series.Count, Is.EqualTo(2));
          Assert.That(profileViewSet.SerieSets.First().Series.First().SeriesName, Is.EqualTo(serieName1));
          Assert.That(profileViewSet.SerieSets.First().Series.First().Values, Is.EqualTo(new double[] { 4, 5 }));
          Assert.That(profileViewSet.SerieSets.First().Series.Last().SeriesName, Is.EqualTo(serieName2));
          Assert.That(profileViewSet.SerieSets.First().Series.Last().Values, Is.EqualTo(new double[] { 6, 7 }));
        }

        [Test]
        public void GetProfileViewSetMergeCategories()
        {
          // Arrange
          var serieName1 = new SeriesName("TheLabel", ObisCode.ElectrActualPowerP14);
          var serieName2 = new SeriesName("TheLabel", ObisCode.ElectrActualPowerP23);
          ProfileGraph profileGraph = new ProfileGraph("day", "MyPage", "MyTitle", "5-minutes", 0, new[] { serieName1, serieName2 });
          var profileGraphs = new[] { profileGraph };
          var ts1 = new DateTime(2000, 1, 2, 3, 5, 0, DateTimeKind.Utc);
          var ts2 = new DateTime(2000, 1, 2, 3, 10, 0, DateTimeKind.Utc);
          var ts3 = new DateTime(2000, 1, 2, 3, 15, 0, DateTimeKind.Utc);
          var labelProfile = new LabelProfile(serieName1.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName1.ObisCode, new [] { new SV("1", ts1, 4, w), new SV("1", ts2, 5, w) }},
            { serieName2.ObisCode, new [] { new SV("1", ts2, 4, w), new SV("1", ts3, 5, w) }}
          });
          var target = CreateTarget(DateTime.UtcNow, new[] { labelProfile });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(1));
          Assert.That(profileViewSet.SerieSets.First().Categories, Is.EqualTo(new[] { ts1, ts2, ts3 }));
          Assert.That(profileViewSet.SerieSets.First().Series.Count, Is.EqualTo(2));
          Assert.That(profileViewSet.SerieSets.First().Series.First().Values.Length, Is.EqualTo(3));
          Assert.That(profileViewSet.SerieSets.First().Series.Last().Values.Length, Is.EqualTo(3));
        }

        [Test]
        public void GetProfileViewSetTwoGraphs()
        {
          // Arrange
          var serieName1 = new SeriesName("TheLabel1", ObisCode.ElectrActualPowerP14);
          var serieName2 = new SeriesName("TheLabel2", ObisCode.ElectrActualPowerP23);
          ProfileGraph profileGraph1 = new ProfileGraph("day", "MyPage", "MyTitle1", "5-minutes", 1, new[] { serieName1 });
          ProfileGraph profileGraph2 = new ProfileGraph("day", "MyPage", "MyTitle2", "5-minutes", 2, new[] { serieName2 });
          var profileGraphs = new[] { profileGraph1, profileGraph2 };
          var ts1 = new DateTime(2000, 1, 2, 3, 5, 0, DateTimeKind.Utc);
          var ts2 = new DateTime(2000, 1, 2, 3, 10, 0, DateTimeKind.Utc);
          var labelProfile1 = new LabelProfile(serieName1.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName1.ObisCode, new [] { new SV("1", ts1, 4, w), new SV("1", ts2, 5, w) }}
          });
          var labelProfile2 = new LabelProfile(serieName2.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName2.ObisCode, new [] { new SV("1", ts1, 6, w), new SV("1", ts2, 7, w) }}
          });
          var target = CreateTarget(DateTime.UtcNow, new[] { labelProfile1, labelProfile2 });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.SerieSets.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetProfileViewSetInterimPeriodTotals()
        {
          // Arrange
          var serieName1 = new SeriesName("TheLabel1", ObisCode.ElectrActiveEnergyA14Period);
          var serieName2 = new SeriesName("TheLabel2", ObisCode.ElectrActiveEnergyA14Period);
          var serieName3 = new SeriesName("TheLabel3", ObisCode.ElectrActualPowerP14);
          ProfileGraph profileGraph1 = new ProfileGraph("day", "MyPage", "MyTitle1", "5-minutes", 1, new[] { serieName1 });
          ProfileGraph profileGraph2 = new ProfileGraph("day", "MyPage", "MyTitle2", "5-minutes", 2, new[] { serieName2 });
          ProfileGraph profileGraph3 = new ProfileGraph("day", "MyPage", "MyTitle3", "5-minutes", 3, new[] { serieName3 });
          var profileGraphs = new[] { profileGraph1, profileGraph2, profileGraph3 };
          var ts1 = new DateTime(2000, 1, 2, 3, 5, 0, DateTimeKind.Utc);
          var ts2 = new DateTime(2000, 1, 2, 3, 10, 0, DateTimeKind.Utc);
          var labelProfile1 = new LabelProfile(serieName1.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName1.ObisCode, new [] { new SV("1", ts1, 4, wh), new SV("1", ts2, 5, wh) }}
          });
          var labelProfile2 = new LabelProfile(serieName2.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName2.ObisCode, new [] { new SV("1", ts1, 6, wh), new SV("1", ts2, 7, wh) }}
          });
          var labelProfile3 = new LabelProfile(serieName3.Label, ts1, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>{
            { serieName3.ObisCode, new [] { new SV("1", ts1, 8, w), new SV("1", ts2, 9, w) }}
          });
          var target = CreateTarget(DateTime.UtcNow, new[] { labelProfile1, labelProfile2, labelProfile3 });

          // Act
          var profileViewSet = target.GetProfileViewSet(profileGraphs);

          // Assert
          Assert.That(profileViewSet.PeriodTotals.Count, Is.EqualTo(2));
          Assert.That(profileViewSet.PeriodTotals.First().SerieName, Is.EqualTo(serieName1));
          Assert.That(profileViewSet.PeriodTotals.First().UnitValue.Value, Is.EqualTo(5));
          Assert.That(profileViewSet.PeriodTotals.Last().SerieName, Is.EqualTo(serieName2));
          Assert.That(profileViewSet.PeriodTotals.Last().UnitValue.Value, Is.EqualTo(7));
        }
    */
    /*
        [Test]
        public void GenerateFromTemplates()
        {
          // Arrange
          var target = CreateTarget(DateTime.UtcNow, new LabelProfile[0]);
          var templateExpression = new Mock<ITemplateExpression>();
          ObisCode obisCode = "1.2.3.4.5.255";
          var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate(obisCode, templateExpression.Object) }) };
          templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string,ICollection<ObisCode>>>())).Returns(true);
          var valueExpressionSet = new Mock<IValueExpressionSet>();
          templateExpression.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime,DateTime>>()))
                                   .Returns(valueExpressionSet.Object);
          var trv1 = new SV("1", DateTime.UtcNow.AddMinutes(1), 7, Unit.WattHour);
          var trv2 = new SV("1", DateTime.UtcNow.AddMinutes(20), 9, Unit.WattHour);
          valueExpressionSet.Setup(sve => sve.Evaluate())
                            .Returns(new [] { new CoarseTimeRegisterValue(trv2.Timestamp, trv2), new CoarseTimeRegisterValue(trv1.Timestamp, trv1) });

          // Act
          target.GenerateFromTemplates(labelObisCodeTemplates, "5-minutes");

          // Assert
          templateExpression.Verify(te => te.GetValueExpressionSet(It.Is<LabelProfileSet>(x => x == target), It.IsAny<Func<DateTime, DateTime>>()));
          Assert.That(target.Count(), Is.EqualTo(1));
          var newLabelProfile = target.First();
          Assert.That(newLabelProfile.Label, Is.EqualTo("NewLabel"));
          Assert.That(newLabelProfile.ToArray(), Is.EqualTo(new [] {obisCode}));
          Assert.That(newLabelProfile[obisCode].Count(), Is.EqualTo(2));
          Assert.That(newLabelProfile[obisCode], Is.EqualTo(new [] { trv1, trv2 }));
        }

        [Test]
        public void GenerateFromTemplatesIsSatisfiedFalse()
        {
          // Arrange
          var target = CreateTarget(DateTime.UtcNow, new LabelProfile[0]);
          var templateExpression = new Mock<ITemplateExpression>();
          var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate("1.100.1.8.0.255", templateExpression.Object) }) };
          templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string,ICollection<ObisCode>>>())).Returns(false);

          // Act
          target.GenerateFromTemplates(labelObisCodeTemplates, "5-minutes");

          // Assert
          Assert.That(target.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GenerateFromTemplatesGetValueExpressionSetThrows()
        {
          // Arrange
          var target = CreateTarget(DateTime.UtcNow, new LabelProfile[0]);
          var templateExpression = new Mock<ITemplateExpression>();
          var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate("1.100.1.8.0.255", templateExpression.Object) }) };
          templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string,ICollection<ObisCode>>>())).Returns(true);
          templateExpression.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime,DateTime>>())).Throws(new ValueExpressionSetException());

          // Act
          target.GenerateFromTemplates(labelObisCodeTemplates, "5-minutes");

          // Assert
          Assert.That(target.Count(), Is.EqualTo(0));
        }
    */
    private static LabelSeriesSet CreateTarget(DateTime start, DateTime end, IList<LabelSeries> labelSeries)
    {
      return new LabelSeriesSet(start, end, labelSeries);
    }

  }
}

