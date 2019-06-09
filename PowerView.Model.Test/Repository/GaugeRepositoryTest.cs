using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class GaugeRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void GetThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetLatest(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetLatest()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd1 = new Db.LiveReading { Label="L1", SerialNumber="1", Timestamp=dt };
      var rd2 = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt };
      var rd3 = new Db.MonthReading { Label="L1", SerialNumber="1", Timestamp=dt };
      var rd4 = new Db.YearReading { Label="L1", SerialNumber="1", Timestamp=dt };
      Insert(rd1);
      Insert(rd2);
      Insert(rd3);
      Insert(rd4);
      Insert(new Db.LiveRegister { ReadingId=rd1.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.DayRegister { ReadingId=rd2.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.MonthRegister { ReadingId=rd3.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.YearRegister { ReadingId=rd4.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetLatest(dt);

      // Assert
      Assert.That(values.Count, Is.EqualTo(4));
      Assert.That(values.First().Name, Is.EqualTo(GaugeSetName.Latest));
      Assert.That(values.First().GuageValues.Count, Is.EqualTo(1));
      Assert.That(values.Skip(1).First().Name, Is.EqualTo(GaugeSetName.LatestDay));
      Assert.That(values.Skip(1).First().GuageValues.Count, Is.EqualTo(1));
      Assert.That(values.Skip(2).First().Name, Is.EqualTo(GaugeSetName.LatestMonth));
      Assert.That(values.Skip(2).First().GuageValues.Count, Is.EqualTo(1));
      Assert.That(values.Last().Name, Is.EqualTo(GaugeSetName.LatestYear));
      Assert.That(values.Last().GuageValues.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetLatestGroupsByLabelObisCodeAndSerialNumber()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd1 = new Db.LiveReading { Label="L1", SerialNumber="1", Timestamp=dt };
      var rd2 = new Db.LiveReading { Label="L1", SerialNumber="2", Timestamp=dt };
      var rd3 = new Db.LiveReading { Label="L2", SerialNumber="2", Timestamp=dt };
      var rd4 = new Db.LiveReading { Label="L2", SerialNumber="2", Timestamp=dt };
      Insert(rd1, rd2, rd3, rd4);
      Insert(new Db.LiveRegister { ReadingId=rd1.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.LiveRegister { ReadingId=rd2.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.LiveRegister { ReadingId=rd3.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.LiveRegister { ReadingId=rd4.Id, ObisCode = ObisCode.ActiveEnergyA23, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetLatest(dt);

      // Assert
      Assert.That(values.Count, Is.EqualTo(1));
      Assert.That(values.First().GuageValues.Count, Is.EqualTo(4));
    }

    [Test]
    public void GetLatestFiltersOnTimestamp()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd = new Db.LiveReading { Label="L1", SerialNumber="1", Timestamp=dt - TimeSpan.FromDays(2) };
      Insert(rd);
      Insert(new Db.LiveRegister { ReadingId=rd.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetLatest(dt);

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetLatestFiltersOnCumulatingObisCodes()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd = new Db.LiveReading { Label="L1", SerialNumber="1", Timestamp=dt };
      Insert(rd);
      Insert(new Db.LiveRegister { ReadingId=rd.Id, ObisCode = ObisCode.ActualPowerP14, Value = 3, Scale = 1, Unit = (byte)Unit.Watt });

      // Act
      var values = target.GetLatest(dt);

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetCustom()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt };
      Insert(rd);
      Insert(new Db.DayRegister { ReadingId=rd.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetCustom(dt.AddHours(1));

      // Assert
      Assert.That(values.Count, Is.EqualTo(1));
      Assert.That(values.First().Name, Is.EqualTo(GaugeSetName.Custom));
      Assert.That(values.First().GuageValues.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetCustomGroupsByLabelObisCodeAndSerialNumber()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd1 = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt };
      var rd2 = new Db.DayReading { Label="L1", SerialNumber="2", Timestamp=dt };
      var rd3 = new Db.DayReading { Label="L2", SerialNumber="2", Timestamp=dt };
      var rd4 = new Db.DayReading { Label="L2", SerialNumber="2", Timestamp=dt };
      Insert(rd1, rd2, rd3, rd4);
      Insert(new Db.DayRegister { ReadingId=rd1.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.DayRegister { ReadingId=rd2.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.DayRegister { ReadingId=rd3.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.DayRegister { ReadingId=rd4.Id, ObisCode = ObisCode.ActiveEnergyA23, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetCustom(dt.AddHours(1));

      // Assert
      Assert.That(values.Count, Is.EqualTo(1));
      Assert.That(values.First().GuageValues.Count, Is.EqualTo(4));
    }

    [Test]
    public void GetCustomFiltersOnTimestamp()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd1 = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt - TimeSpan.FromDays(3) };
      var rd2 = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt };
      Insert(rd1, rd2);
      Insert(new Db.DayRegister { ReadingId=rd1.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour });
      Insert(new Db.DayRegister { ReadingId=rd2.Id, ObisCode = ObisCode.ActiveEnergyA14, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour });

      // Act
      var values = target.GetCustom(dt);

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetCustomFiltersOnCumulatingObisCodes()
    {
      // Arrange
      var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
      var target = CreateTarget();
      var rd = new Db.DayReading { Label="L1", SerialNumber="1", Timestamp=dt };
      Insert(rd);
      Insert(new Db.DayRegister { ReadingId=rd.Id, ObisCode = ObisCode.ActualPowerP14, Value = 3, Scale = 1, Unit = (byte)Unit.Watt });

      // Act
      var values = target.GetCustom(dt.AddHours(1));

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }


    private void Insert<T>(params T[] entities) where T : class, IDbEntity
    {
      foreach (var entity in entities)
      {
        DbContext.Connection.Insert(entity);
      }
    }


    private GaugeRepository CreateTarget()
    {
      return new GaugeRepository(DbContext);
    }

  }
}
