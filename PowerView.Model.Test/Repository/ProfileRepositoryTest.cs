using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class ProfileRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void GetDayProfileThrows()
        {
            // Arrange
            var dtLocal = DateTime.Now;
            var dtUtc = DateTime.UtcNow;
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetDayProfileSet(dtLocal, dtUtc, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetDayProfileSet(dtUtc, dtLocal, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetDayProfileSet(dtUtc, dtUtc, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetDayProfileOutsideTimestampFilter()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
                new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetDayProfileSet(timestamp + TimeSpan.FromHours(1), timestamp + TimeSpan.FromHours(2), timestamp + TimeSpan.FromHours(3));

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetDayProfileForOneLabel()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 11, 23, 55, 0, DateTimeKind.Local).ToUniversalTime();
            byte obisCode1 = 1;
            byte obisCode11 = 11;
            DbContext.InsertObisCodes((obisCode1, "1.1.1.1.1.1"), (obisCode11, "11.11.11.11.11.11"));
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = obisCode1, Value = 1, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisId = obisCode11, Value = 11, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisId = obisCode1, Value = 2, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisId = obisCode11, Value = 22, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromMinutes(10) },
              new Db.LiveRegister { ObisId = obisCode1, Value = 3, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp + TimeSpan.FromMinutes(5);
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetDayProfileSet(timestamp, start, end);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
            var labelProfile = labelSeriesSet.First();
            Assert.That(labelProfile.Label, Is.EqualTo(labels.First()));
            Assert.That(labelProfile.Count(), Is.EqualTo(2));
            Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
            Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetMonthProfileThrows()
        {
            // Arrange
            var dtLocal = DateTime.Now;
            var dtUtc = DateTime.UtcNow;
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetMonthProfileSet(dtLocal, dtUtc, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetMonthProfileSet(dtUtc, dtLocal, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetMonthProfileSet(dtUtc, dtUtc, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetMonthProfileOutsideTimestampFilter()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            DbContext.Insert(new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.DayRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetMonthProfileSet(timestamp + TimeSpan.FromDays(1), timestamp + TimeSpan.FromDays(2), timestamp + TimeSpan.FromDays(3));

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetMonthProfileForOneLabel()
        {
            // Arrange
            var timestamp = new DateTime(2015, 01, 31, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte obisCode1 = 1;
            byte obisCode11 = 11;
            DbContext.InsertObisCodes((obisCode1, "1.1.1.1.1.1"), (obisCode11, "11.11.11.11.11.11"));
            (var labels, var _) = DbContext.Insert(new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.DayRegister { ObisId = obisCode1, Value = 1, Unit = (byte)Unit.WattHour },
              new Db.DayRegister { ObisId = obisCode11, Value = 11, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromDays(1) },
              new Db.DayRegister { ObisId = obisCode1, Value = 2, Unit = (byte)Unit.WattHour },
              new Db.DayRegister { ObisId = obisCode11, Value = 22, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromDays(2) },
              new Db.DayRegister { ObisId = obisCode1, Value = 3, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp + TimeSpan.FromDays(1);
            var end = start + TimeSpan.FromDays(28);

            // Act
            var labelSeriesSet = target.GetMonthProfileSet(timestamp, start, end);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
            var labelProfile = labelSeriesSet.First();
            Assert.That(labelProfile.Label, Is.EqualTo(labels.First()));
            Assert.That(labelProfile.Count(), Is.EqualTo(2));
            Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
            Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetYearProfileThrows()
        {
            // Arrange
            var dtLocal = DateTime.Now;
            var dtUtc = DateTime.UtcNow;
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetYearProfileSet(dtLocal, dtUtc, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetYearProfileSet(dtUtc, dtLocal, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetYearProfileSet(dtUtc, dtUtc, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetYearProfileOutsideTimestampFilter()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            DbContext.Insert(new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.MonthRegister { ObisId = electrActiveEnergyA14, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetYearProfileSet(timestamp.AddMonths(1), timestamp.AddMonths(2), timestamp.AddMonths(3));

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetYearProfileForOneLabel()
        {
            // Arrange
            var timestamp = new DateTime(2014, 12, 01, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte obisCode1 = 1;
            byte obisCode11 = 11;
            DbContext.InsertObisCodes((obisCode1, "1.1.1.1.1.1"), (obisCode11, "11.11.11.11.11.11"));
            (var labels, var _) = DbContext.Insert(new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.MonthRegister { ObisId = obisCode1, Value = 1, Unit = (byte)Unit.WattHour },
              new Db.MonthRegister { ObisId = obisCode11, Value = 11, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp.AddMonths(1) },
              new Db.MonthRegister { ObisId = obisCode1, Value = 2, Unit = (byte)Unit.WattHour },
              new Db.MonthRegister { ObisId = obisCode11, Value = 22, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp.AddMonths(2) },
              new Db.MonthRegister { ObisId = obisCode1, Value = 3, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp.AddMonths(1);
            var end = start.AddMonths(12);

            // Act
            var labelSeriesSet = target.GetYearProfileSet(timestamp, start, end);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
            var labelProfile = labelSeriesSet.First();
            Assert.That(labelProfile.Label, Is.EqualTo(labels.First()));
            Assert.That(labelProfile.Count(), Is.EqualTo(2));
            Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
            Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
        }

    [Test]
    public void GetDecadeProfileThrows()
    {
      // Arrange
      var dtLocal = DateTime.Now;
      var dtUtc = DateTime.UtcNow;
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetDecadeProfileSet(dtLocal, dtUtc, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetDecadeProfileSet(dtUtc, dtLocal, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetDecadeProfileSet(dtUtc, dtUtc, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetDecadeProfileOutsideTimestampFilter()
    {
      // Arrange
      var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      byte electrActiveEnergyA14 = 1;
      DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
      DbContext.Insert(new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
          new Db.YearRegister { ObisId = electrActiveEnergyA14, Value = 1 });
      var target = CreateTarget();

      // Act
      var labelSeriesSet = target.GetDecadeProfileSet(timestamp.AddYears(1), timestamp.AddYears(2), timestamp.AddYears(12));

      // Assert
      Assert.That(labelSeriesSet, Is.Not.Null);
      Assert.That(labelSeriesSet, Is.Empty);
    }

    [Test]
    public void GetDecadeProfileForOneLabel()
    {
      // Arrange
      // Arrange
      var timestamp = new DateTime(2014, 12, 01, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      byte obisCode1 = 1;
      byte obisCode11 = 11;
      DbContext.InsertObisCodes((obisCode1, "1.1.1.1.1.1"), (obisCode11, "11.11.11.11.11.11"));
      (var labels, var _) = DbContext.Insert(new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
        new Db.YearRegister { ObisId = obisCode1, Value = 1, Unit = (byte)Unit.WattHour },
        new Db.YearRegister { ObisId = obisCode11, Value = 11, Unit = (byte)Unit.Watt });
      DbContext.Insert(new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp.AddYears(1) },
        new Db.YearRegister { ObisId = obisCode1, Value = 2, Unit = (byte)Unit.WattHour },
        new Db.YearRegister { ObisId = obisCode11, Value = 22, Unit = (byte)Unit.Watt });
      DbContext.Insert(new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp.AddYears(2) },
        new Db.YearRegister { ObisId = obisCode1, Value = 3, Unit = (byte)Unit.WattHour });
      var target = CreateTarget();
      var start = timestamp.AddYears(1);
      var end = start.AddYears(11);

      // Act
      var labelSeriesSet = target.GetDecadeProfileSet(timestamp, start, end);

      // Assert
      Assert.That(labelSeriesSet, Is.Not.Null);
      Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
      Assert.That(labelSeriesSet.End, Is.EqualTo(end));
      Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
      var labelProfile = labelSeriesSet.First();
      Assert.That(labelProfile.Label, Is.EqualTo(labels.First()));
      Assert.That(labelProfile.Count(), Is.EqualTo(2));
      Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
      Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
    }

    private ProfileRepository CreateTarget()
        {
            return new ProfileRepository(DbContext);
        }

    }
}
