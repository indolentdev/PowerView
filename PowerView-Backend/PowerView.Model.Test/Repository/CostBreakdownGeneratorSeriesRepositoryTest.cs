using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class CostBreakdownGeneratorSeriesRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new CostBreakdownGeneratorSeriesRepository(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetCostBreakdownGeneratorSeriesAbsent()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var result = target.GetCostBreakdownGeneratorSeries();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        [TestCase("1.68.25.67.0.255", "1.69.25.67.0.255")]
        [TestCase("1.68.25.68.0.255", "1.69.25.68.0.255")]
        public void GetCostBreakdownGeneratorSeriesNoEntries(string obisCode, string genObisCode)
        {
            // Arrange
            var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
            var costBreakdownDb = new Db.CostBreakdown { Title = "Title", Currency = (int)Unit.Eur, Vat = 10 };
            DbContext.InsertCostBreakdown(costBreakdownDb);
            var labels = DbContext.InsertLabels("lbl1");
            var obisCodes = DbContext.InsertObisCodes(obisCode);
            var generatorSeriesDb = new Db.GeneratorSeries
            {
                Label = "lbl",
                ObisCode = (ObisCode)genObisCode,
                BaseLabelId = labels[0].Id,
                BaseObisId = obisCodes[0].Id,
                CostBreakdownTitle = costBreakdownDb.Title
            };
            DbContext.InsertGeneratorSeries(generatorSeriesDb);
            var target = CreateTarget();

            // Act
            var result = target.GetCostBreakdownGeneratorSeries();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            CostBreakdownRepositoryTest.AssertCostBreakdown(costBreakdownDb, Array.Empty<Db.CostBreakdownEntry>(), result.First().CostBreakdown);
            GeneratorSeriesRepositoryTest.AssertGeneratorSeries(new SeriesName("lbl", genObisCode),
              new SeriesName("lbl1", obisCode), costBreakdownDb.Title, result.First().GeneratorSeries);
        }

        [Test]
        [TestCase("1.68.25.67.0.255", "1.69.25.67.0.255")]
        [TestCase("1.68.25.68.0.255", "1.69.25.68.0.255")]
        public void GetCostBreakdownGeneratorSeriesWithEntries(string obisCode, string genObisCode)
        {
            // Arrange
            var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
            var costBreakdownDb = new Db.CostBreakdown { Title = "Title", Currency = (int)Unit.Eur, Vat = 10 };
            var costBreakdownEntryDb1 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
            var costBreakdownEntryDb2 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(8), Name = "N12", StartTime = 3, EndTime = 21, Amount = 2.345678 };
            DbContext.InsertCostBreakdown(costBreakdownDb, costBreakdownEntryDb1, costBreakdownEntryDb2);
            var labels = DbContext.InsertLabels("lbl1");
            var obisCodes = DbContext.InsertObisCodes(obisCode);
            var generatorSeriesDb = new Db.GeneratorSeries
            {
                Label = "lbl",
                ObisCode = (ObisCode)genObisCode,
                BaseLabelId = labels[0].Id,
                BaseObisId = obisCodes[0].Id,
                CostBreakdownTitle = costBreakdownDb.Title
            };
            DbContext.InsertGeneratorSeries(generatorSeriesDb);
            var target = CreateTarget();

            // Act
            var result = target.GetCostBreakdownGeneratorSeries();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            CostBreakdownRepositoryTest.AssertCostBreakdown(costBreakdownDb, new[] { costBreakdownEntryDb1, costBreakdownEntryDb2 }, result.First().CostBreakdown);
            GeneratorSeriesRepositoryTest.AssertGeneratorSeries(new SeriesName("lbl", genObisCode),
              new SeriesName("lbl1", obisCode), costBreakdownDb.Title, result.First().GeneratorSeries);
        }

        [Test]
        [TestCase("TitleA", "TitleB")]
        [TestCase("TitleA", "Titlea")]
        public void GetCostBreakdownGeneratorSeriesNoTitleMatch(string title1, string title2)
        {
            // Arrange
            var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
            var costBreakdownDb = new Db.CostBreakdown { Title = title1, Currency = (int)Unit.Eur, Vat = 10 };
            DbContext.InsertCostBreakdown(costBreakdownDb);
            var labels = DbContext.InsertLabels("lbl1");
            var obisCodes = DbContext.InsertObisCodes(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatH);
            var generatorSeriesDb = new Db.GeneratorSeries
            {
                Label = "lbl",
                ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVatH,
                BaseLabelId = labels[0].Id,
                BaseObisId = obisCodes[0].Id,
                CostBreakdownTitle = title2
            };
            DbContext.InsertGeneratorSeries(generatorSeriesDb);
            var target = CreateTarget();

            // Act
            var result = target.GetCostBreakdownGeneratorSeries();

            // Assert
            Assert.That(result, Is.Empty);
        }

        private CostBreakdownGeneratorSeriesRepository CreateTarget()
        {
            return new CostBreakdownGeneratorSeriesRepository(DbContext);
        }

    }
}
