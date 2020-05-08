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
      Insert(new Db.LiveReading { Label = "TheLabel", DeviceId = "1", Timestamp = timestamp },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
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
      const string label = "TheLabel";
      var timestamp = new DateTime(2015, 02, 11, 23, 55, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit = (byte)Unit.WattHour },
        new Db.LiveRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit = (byte)Unit.Watt });
      Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromMinutes(5) },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit = (byte)Unit.WattHour },
        new Db.LiveRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit = (byte)Unit.Watt });
      Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromMinutes(10) },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit = (byte)Unit.WattHour });
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
      Assert.That(labelProfile.Label, Is.EqualTo(label));
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
      Insert(new Db.DayReading { Label = "TheLabel", DeviceId = "1", Timestamp = timestamp },
        new Db.DayRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
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
      const string label = "TheLabel";
      var timestamp = new DateTime(2015, 01, 31, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.DayReading { Label = label, DeviceId = "1", Timestamp = timestamp },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit = (byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit = (byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromDays(1) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit = (byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit = (byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromDays(2) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit = (byte)Unit.WattHour });
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
      Assert.That(labelProfile.Label, Is.EqualTo(label));
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
      Insert(new Db.MonthReading { Label = "TheLabel", DeviceId = "1", Timestamp = timestamp },
        new Db.MonthRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
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
      const string label = "TheLabel";
      var timestamp = new DateTime(2014, 12, 01, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.MonthReading { Label = label, DeviceId = "1", Timestamp = timestamp },
        new Db.MonthRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit = (byte)Unit.WattHour },
        new Db.MonthRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit = (byte)Unit.Watt });
      Insert(new Db.MonthReading { Label = label, DeviceId = "1", Timestamp = timestamp.AddMonths(1) },
        new Db.MonthRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit = (byte)Unit.WattHour },
        new Db.MonthRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit = (byte)Unit.Watt });
      Insert(new Db.MonthReading { Label = label, DeviceId = "1", Timestamp = timestamp.AddMonths(2) },
        new Db.MonthRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit = (byte)Unit.WattHour });
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
      Assert.That(labelProfile.Label, Is.EqualTo(label));
      Assert.That(labelProfile.Count(), Is.EqualTo(2));
      Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
      Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
    }

    private void Insert<TReading, TRegister>(TReading reading, params TRegister[] registers) 
      where TReading : class, IDbReading
      where TRegister : class, IDbRegister
    {
      DbContext.InsertReadings(reading);
      foreach (var register in registers)
      {
        register.ReadingId = reading.Id;
      }
      DbContext.InsertRegisters(registers);
    }

    private ProfileRepository CreateTarget()
    {
      return new ProfileRepository(DbContext);
    }

  }
}
