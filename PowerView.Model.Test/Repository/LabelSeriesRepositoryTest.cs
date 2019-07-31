using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class LabelSeriesRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void GetDayProfileThrows()
    {
      // Arrange
      var dtLocal = DateTime.Now;
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetDayLabelSeriesSet(dtLocal, DateTime.UtcNow, DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetDayLabelSeriesSet(DateTime.UtcNow, dtLocal, DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetDayLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
    }


    [Test]
    public void GetDayLabelSeriesSetOutsideTimestampFilter()
    {
      // Arrange
      var timestamp = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      Insert(new Db.LiveReading { Label = "TheLabel", SerialNumber = "1", Timestamp = timestamp },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
      var target = CreateTarget();

      // Act
      var labelSeriesSet = target.GetDayLabelSeriesSet(timestamp+TimeSpan.FromDays(1), timestamp + TimeSpan.FromDays(2), timestamp + TimeSpan.FromDays(3));

      // Assert
      Assert.That(labelSeriesSet, Is.Not.Null);
      Assert.That(labelSeriesSet, Is.Empty);
    }

    [Test]
    public void GetDayLabelSeriesForOneLabel()
    {
      // Arrange
      const string label = "TheLabel";
      var timestamp = new DateTime(2015, 02, 12, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.LiveReading { Label = label, SerialNumber = "1", Timestamp = timestamp },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit=(byte)Unit.WattHour },
        new Db.LiveRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit=(byte)Unit.Watt });
      Insert(new Db.LiveReading { Label = label, SerialNumber = "1", Timestamp = timestamp+TimeSpan.FromMinutes(5) },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit=(byte)Unit.WattHour },
        new Db.LiveRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit=(byte)Unit.Watt });
      Insert(new Db.LiveReading { Label = label, SerialNumber = "1", Timestamp = timestamp+TimeSpan.FromMinutes(10) },
        new Db.LiveRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit=(byte)Unit.WattHour });
      var target = CreateTarget();

      // Act
      var labelSeriesSet = target.GetDayLabelSeriesSet(timestamp - TimeSpan.FromMinutes(5), timestamp, timestamp + TimeSpan.FromDays(1));

      // Assert
      Assert.That(labelSeriesSet, Is.Not.Null);
      Assert.That(labelSeriesSet.Start, Is.EqualTo(timestamp));
      Assert.That(labelSeriesSet.End, Is.EqualTo(timestamp + TimeSpan.FromDays(1)));
      Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
      var labelSeries = labelSeriesSet.First();
      Assert.That(labelSeries.Label, Is.EqualTo(label));
      Assert.That(labelSeries.Count(), Is.EqualTo(2));
      Assert.That(labelSeries["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
      Assert.That(labelSeries["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
    }
/*
    [Test]
    public void GetMonthProfileThrows()
    {
      // Arrange
      var dtLocal = DateTime.Now;
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetMonthProfileSet(dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetMonthProfileOutsideTimestampFilter()
    {
      // Arrange
      var start = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      Insert(new Db.DayReading { Label = "TheLabel", SerialNumber = "1", Timestamp = start },
        new Db.DayRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetMonthProfileSet(start+TimeSpan.FromDays(2));

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet, Is.Empty);
    }

    [Test]
    public void GetMonthProfileForOneLabel()
    {
      // Arrange
      const string label = "TheLabel";
      var start = new DateTime(2015, 02, 12, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = start },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit=(byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit=(byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = start+TimeSpan.FromMinutes(5) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit=(byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit=(byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = start+TimeSpan.FromMinutes(10) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit=(byte)Unit.WattHour });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetMonthProfileSet(start);

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet.Start, Is.EqualTo(start));
      Assert.That(labelProfileSet.Count(), Is.EqualTo(1));
      var labelProfile = labelProfileSet.First();
      Assert.That(labelProfile.Label, Is.EqualTo(label));
      Assert.That(labelProfile.Count(), Is.EqualTo(2));
      Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
      Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
    }

    [Test]
    public void GetMonthProfileStartLastDayOfMonth()
    {
      // Arrange
      const string label = "TheLabel";
      var start = new DateTime(2015, 09, 30, 22, 0, 0, DateTimeKind.Utc);
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp=new DateTime(2015, 09, 30, 21, 55, 0, DateTimeKind.Utc) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 10, Unit=(byte)Unit.WattHour });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp=new DateTime(2015, 10, 01, 22, 0, 0, DateTimeKind.Utc) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 15, Unit=(byte)Unit.WattHour });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp=new DateTime(2015, 10, 31, 22, 55, 0, DateTimeKind.Utc) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 25, Unit=(byte)Unit.WattHour });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetMonthProfileSet(start);

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet.Start, Is.EqualTo(start));
      Assert.That(labelProfileSet.Count(), Is.EqualTo(1));
      var labelProfile = labelProfileSet.First();
      Assert.That(labelProfile.Label, Is.EqualTo(label));
      Assert.That(labelProfile.Count(), Is.EqualTo(1));
      Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(2));
      Assert.That(labelProfile["1.1.1.1.1.1"].Select(sv => sv.Timestamp), Is.EqualTo(new [] { new DateTime(2015, 10, 01, 22, 0, 0, DateTimeKind.Utc), new DateTime(2015, 10, 31, 22, 55, 0, DateTimeKind.Utc) }));
    }

    [Test]
    public void GetCustomProfileThrows()
    {
      // Arrange
      var dtLocal = DateTime.Now;
      var dtUtc = DateTime.UtcNow;
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetCustomProfileSet(dtLocal, dtUtc), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetCustomProfileSet(dtUtc, dtLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetCustomProfileBeforeTimeSpanFilter()
    {
      // Arrange
      var from = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      var to = new DateTime(2016, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      Insert(new Db.DayReading { Label = "TheLabel", SerialNumber = "1", Timestamp = from-TimeSpan.FromDays(1) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetCustomProfileSet(from, to);

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet, Is.Empty);
    }

    [Test]
    public void GetCustomProfileAfterTimeSpanFilter()
    {
      // Arrange
      var from = new DateTime(2015, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      var to = new DateTime(2016, 02, 12, 22, 15, 33, DateTimeKind.Utc);
      Insert(new Db.DayReading { Label = "TheLabel", SerialNumber = "1", Timestamp = to+TimeSpan.FromDays(1) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.0.1.8.0.255", Value = 1 });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetCustomProfileSet(from, to);

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet, Is.Empty);
    }

    [Test]
    public void GetCustomProfileForOneLabel()
    {
      // Arrange
      const string label = "TheLabel";
      var from = new DateTime(2015, 02, 12, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      var to = new DateTime(2015, 02, 15, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = from },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 1, Unit=(byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 11, Unit=(byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = from+TimeSpan.FromDays(1) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 2, Unit=(byte)Unit.WattHour },
        new Db.DayRegister { ObisCode = (ObisCode)"11.11.11.11.11.11", Value = 22, Unit=(byte)Unit.Watt });
      Insert(new Db.DayReading { Label = label, SerialNumber = "1", Timestamp = from+TimeSpan.FromDays(2) },
        new Db.DayRegister { ObisCode = (ObisCode)"1.1.1.1.1.1", Value = 3, Unit=(byte)Unit.WattHour });
      var target = CreateTarget();

      // Act
      var labelProfileSet = target.GetCustomProfileSet(from, to);

      // Assert
      Assert.That(labelProfileSet, Is.Not.Null);
      Assert.That(labelProfileSet.Start, Is.EqualTo(from));
      Assert.That(labelProfileSet.Count(), Is.EqualTo(1));
      var labelProfile = labelProfileSet.First();
      Assert.That(labelProfile.Label, Is.EqualTo(label));
      Assert.That(labelProfile.Count(), Is.EqualTo(2));
      Assert.That(labelProfile["1.1.1.1.1.1"].Count(), Is.EqualTo(3));
      Assert.That(labelProfile["11.11.11.11.11.11"].Count(), Is.EqualTo(2));
    }
*/
    private void Insert<TReading, TRegister>(TReading reading, params TRegister[] registers) 
      where TReading : class, IDbReading
      where TRegister : class, IDbRegister
    {
      DbContext.Connection.Insert(reading);
      foreach (var register in registers)
      {
        object registerAsObject = register;
        var liveRegister = registerAsObject as Db.LiveRegister;
        if (liveRegister != null)
        {
          liveRegister.ReadingId = reading.Id;
        }
        var dayRegister = registerAsObject as Db.DayRegister;
        if (dayRegister != null)
        {
          dayRegister.ReadingId = reading.Id;
        }
        DbContext.Connection.Insert(register);
      }
    }

    private LabelSeriesRepository CreateTarget()
    {
      return new LabelSeriesRepository(DbContext);
    }

  }
}
