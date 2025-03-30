using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using System.Globalization;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class GeneratorSeriesRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new GeneratorSeriesRepository(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetGeneratorSeriesEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var generatorSeries = target.GetGeneratorSeries();

            // Assert
            Assert.That(generatorSeries, Is.Empty);
        }

        [Test]
        public void GetGeneratorSeries()
        {
            // Arrange
            var labels = DbContext.InsertLabels("lbl2", "lbl4");
            var obisCodes = DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            DbContext.InsertGeneratorSeries(
              new Db.GeneratorSeries { Label = "lbl1", ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, BaseLabelId = labels[0].Id, BaseObisId = obisCodes[0].Id, CostBreakdownTitle = "CostBreakdown1" },
              new Db.GeneratorSeries { Label = "lbl3", ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, BaseLabelId = labels[1].Id, BaseObisId = obisCodes[0].Id, CostBreakdownTitle = "CostBreakdown2" });

            var target = CreateTarget();

            // Act
            var generatorSeries = target.GetGeneratorSeries();

            // Assert
            Assert.That(generatorSeries.Count, Is.EqualTo(2));
            AssertGeneratorSeries(new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat),
              new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CostBreakdown1", generatorSeries.First());
            AssertGeneratorSeries(new SeriesName("lbl3", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat),
              new SeriesName("lbl4", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CostBreakdown2", generatorSeries.Last());
        }

        [Test]
        public void AddGeneratorSeriesThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.AddGeneratorSeries(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddGeneratorSeries()
        {
            // Arrange
            DbContext.InsertLabels("lblOther");
            DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            var generatorSeries = new GeneratorSeries(new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat),
              new SeriesName("lblOther", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CostBreakdownTitle1");
            var target = CreateTarget();

            // Act
            target.AddGeneratorSeries(generatorSeries);

            // Assert
            AssertGeneratorSeriesExists(generatorSeries);
        }

        [Test]
        public void AddGeneratorSeriesDuplicate()
        {
            // Arrange
            DbContext.InsertLabels("lblOther");
            DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            var generatorSeries = new GeneratorSeries(new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat),
              new SeriesName("lblOther", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CostBreakdownTitle1");
            var target = CreateTarget();
            target.AddGeneratorSeries(generatorSeries);

            // Act & Assert
            Assert.That(() => target.AddGeneratorSeries(generatorSeries), Throws.TypeOf<DataStoreUniqueConstraintException>());
        }

        [Test]
        public void DeleteGeneratorSeriesThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.DeleteGeneratorSeries(null), Throws.ArgumentNullException);
        }

        [Test]
        public void DeleteGeneratorSeriesAbsent()
        {
            // Arrange
            DbContext.InsertLabels("lbl1");
            DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat);
            var target = CreateTarget();

            // Act
            target.DeleteGeneratorSeries(new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat));

            // Assert
            // Does not throw
        }

        [Test]
        public void DeleteGeneratorSeries()
        {
            // Arrange
            var labels = DbContext.InsertLabels("lbl2", "lbl4");
            var obisCodes = DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            DbContext.InsertGeneratorSeries(
              new Db.GeneratorSeries { Label = "lbl1", ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, BaseLabelId = labels[0].Id, BaseObisId = obisCodes[0].Id, CostBreakdownTitle = "CostBreakdown1" },
              new Db.GeneratorSeries { Label = "lbl3", ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, BaseLabelId = labels[1].Id, BaseObisId = obisCodes[0].Id, CostBreakdownTitle = "CostBreakdown2" });
            var target = CreateTarget();

            // Act
            target.DeleteGeneratorSeries(new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat));

            // Assert
            AssertGeneratorSeriesExists("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, "lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat, "CostBreakdown1", not: true);
            AssertGeneratorSeriesExists("lbl3", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, "lbl4", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat, "CostBreakdown2");
        }

        [Test]
        public void GetBaseSeriesEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var series = target.GetBaseSeries();

            // Assert
            Assert.That(series, Is.Empty);
        }

        [Test]
        public void GetBaseSeries()
        {
            // Arrange
            var labels = DbContext.InsertLabels("lbl1", "lbl2");
            var obis = DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            var dateTime1 = new DateTime(2024, 3, 10, 13, 03, 11, DateTimeKind.Utc);
            var dateTime2 = dateTime1.AddMinutes(4);
            DbContext.InsertLabelObisLive((labels[0].Id, obis[0].Id, (UnixTime)dateTime1), (labels[1].Id, obis[0].Id, (UnixTime)dateTime2));
            var target = CreateTarget();

            // Act
            var items = target.GetBaseSeries();

            // Assert
            Assert.That(items, Is.EquivalentTo(new[] {
        (new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), dateTime1),
        (new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), dateTime2)
      }));
        }

        internal static void AssertGeneratorSeries(SeriesName series, SeriesName baseSeries, string costBreakdownTitle, GeneratorSeries generatorSeries)
        {
            Assert.That(generatorSeries.Series, Is.EqualTo(series));
            Assert.That(generatorSeries.BaseSeries, Is.EqualTo(baseSeries));
            Assert.That(generatorSeries.CostBreakdownTitle, Is.EqualTo(costBreakdownTitle));
        }

        private void AssertGeneratorSeriesExists(string label, ObisCode obisCode, string baseLabel, ObisCode baseObisCode, string costBreakdownTitle, bool not = false)
        {
            var generatorSeriesDb = DbContext.QueryTransaction<Db.GeneratorSeries>(
              @"
        SELECT gs.Label, gs.ObisCode, bl.Id, bo.Id, gs.CostBreakdownTitle
        FROM GeneratorSeries gs
        JOIN Label bl ON gs.BaseLabelId = bl.Id
        JOIN Obis bo ON gs.BaseObisId = bo.Id
        WHERE gs.Label=@Label AND gs.ObisCode=@ObisCode AND bl.LabelName=@BaseLabel AND bo.ObisCode=@BaseObisCode AND gs.CostBreakdownTitle=@CostBreakdownTitle;",
              new { Label = label, ObisCode = (long)obisCode, BaseLabel = baseLabel, BaseObisCode = (long)baseObisCode, CostBreakdownTitle = costBreakdownTitle });

            Assert.That(generatorSeriesDb.Count, Is.EqualTo(not ? 0 : 1));
        }

        private void AssertGeneratorSeriesExists(GeneratorSeries generatorSeries, bool not = false)
        {
            AssertGeneratorSeriesExists(generatorSeries.Series.Label, generatorSeries.Series.ObisCode,
              generatorSeries.BaseSeries.Label, generatorSeries.BaseSeries.ObisCode, generatorSeries.CostBreakdownTitle, not);
        }

        private GeneratorSeriesRepository CreateTarget()
        {
            return new GeneratorSeriesRepository(DbContext);
        }

    }
}
