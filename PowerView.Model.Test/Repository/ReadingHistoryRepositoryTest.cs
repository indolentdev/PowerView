using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class ReadingHistoryRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void ClearDayMonthYearHistory()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
                new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            DbContext.Insert(new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
                new Db.DayRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            DbContext.Insert(new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
                new Db.MonthRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            DbContext.Insert(new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
                new Db.YearRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            DbContext.ExecuteTransaction("INSERT INTO StreamPosition (StreamName, LabelId, Position) VALUES (@StreamName, @LabelId, @Position);",
                new Db.StreamPosition { StreamName = "DayReading", LabelId = 1, Position = 12345 });
            var target = CreateTarget();

            // Act
            target.ClearDayMonthYearHistory();

            // Assert
            Assert.That(DbContext.QueryTransaction("SELECT * FROM YearReading"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM YearRegister"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM MonthReading"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM MonthRegister"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM DayReading"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM DayRegister"), Is.Empty);
            Assert.That(DbContext.QueryTransaction("SELECT * FROM StreamPosition"), Is.Empty);
        }

        private ReadingHistoryRepository CreateTarget()
        {
            return new ReadingHistoryRepository(DbContext);
        }

    }
}
