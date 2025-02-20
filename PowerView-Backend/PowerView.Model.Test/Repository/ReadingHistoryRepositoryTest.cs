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

        [Test]
        public void GetReadingPipeStatus()
        {
            // Arrange
            var target = CreateTarget();
            var now = new DateTime(2022, 4, 29, 11, 22, 33, DateTimeKind.Utc);
            var dt1 = now;
            var dt2 = now.AddMinutes(10);
            var reading1 = new Db.LiveReading { LabelId = 1, DeviceId = 2, Timestamp = dt1 };
            var reading2 = new Db.LiveReading { LabelId = 2, DeviceId = 3, Timestamp = dt2 };
            var (labelsa, _) = DbContext.InsertReadings(reading1, reading2);
            var dt3 = now.AddDays(1);
            var reading3 = new Db.DayReading { LabelId = 10, DeviceId = 20, Timestamp = dt3 };
            var (labelsb, _) = DbContext.InsertReadings(reading3);
            var dt4 = now.AddDays(4);
            var reading4 = new Db.MonthReading { LabelId = 100, DeviceId = 200, Timestamp = dt4 };
            var (labelsc, _) = DbContext.InsertReadings(reading4);
            DbContext.InsertStreamPosition(("DayReading", reading1.LabelId, reading1.Id), ("DayReading", reading2.LabelId, reading2.Id),
              ("MonthReading", reading3.LabelId, reading3.Id), ("YearReading", reading4.LabelId, reading4.Id));

            // Act
            var status = target.GetReadingHistoryStatus();

            // Assert
            Assert.That(status.Count, Is.EqualTo(3));
            var day = status.First(x => x.Interval == "Day");
            Assert.That(day.Status, Is.EquivalentTo(new[] { (labelsa.First(), dt1), (labelsa.Last(), dt2) }));
            var month = status.First(x => x.Interval == "Month");
            Assert.That(month.Status, Is.EquivalentTo(new[] { (labelsb.First(), dt3) }));
            var year = status.First(x => x.Interval == "Year");
            Assert.That(year.Status, Is.EquivalentTo(new[] { (labelsc.First(), dt4) }));
        }


        private ReadingHistoryRepository CreateTarget()
        {
            return new ReadingHistoryRepository(DbContext);
        }

    }
}
