using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class TimeRegisterValueTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var dt = DateTime.Now;

      // Act & Assert
      Assert.That(() => new TimeRegisterValue("1", dt, 1, Unit.Watt), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new TimeRegisterValue("1", dt, 1, 1, Unit.Watt), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var dt = DateTime.UtcNow;

      // Act
      var target = new TimeRegisterValue("1", dt, 100, 2, Unit.WattHour);

      // Assert
      Assert.That(target.SerialNumber, Is.EqualTo("1"));
      Assert.That(target.Timestamp, Is.EqualTo(dt));
      Assert.That(target.UnitValue.Value, Is.EqualTo(10000));
      Assert.That(target.UnitValue.Unit, Is.EqualTo(Unit.WattHour));
    }

    [Test]
    public void SubstractValue()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt.Subtract(TimeSpan.FromMinutes(5)), 20, 1, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 21, 1, Unit.Watt);

      // Act
      var target = t2.SubtractValue(t1); 

      // Assert
      Assert.That(target.Timestamp, Is.EqualTo(dt));
      Assert.That(target.UnitValue.Value, Is.EqualTo(10));
    }

    [Test]
    public void SubstractValueSerialNumberCaseInsensitive()
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("a", dt.Subtract(TimeSpan.FromMinutes(5)), 20, 1, Unit.Watt);
      var t2 = new TimeRegisterValue("A", dt, 21, 1, Unit.Watt);

      // Act
      var target = t2.SubtractValue(t1);

      // Assert
      Assert.That(target.Timestamp, Is.EqualTo(dt));
      Assert.That(target.UnitValue.Value, Is.EqualTo(10));
    }

    [Test]
    public void SubstractValueNegativeAssumeRegisterQuirk()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt.Subtract(TimeSpan.FromMinutes(5)), 102, 0, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 101, 0, Unit.Watt);

      // Act
      var target = t2.SubtractValue(t1); 

      // Assert
      Assert.That(target.UnitValue.Value, Is.EqualTo(0));
    }

    [Test]
    public void SubstractValueNegativeAssumeRegisterWrap()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt.Subtract(TimeSpan.FromMinutes(5)), 880, 0, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 10, 0, Unit.Watt);

      // Act
      var target = t2.SubtractValue(t1); 

      // Assert
      Assert.That(target.UnitValue.Value, Is.EqualTo(1000-880+10));
    }

    [Test]
    public void SubstractValueNegativeThrows()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt.Subtract(TimeSpan.FromMinutes(5)), 880, 0, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 600, 0, Unit.Watt);

      // Act & Assert
      Assert.That(() => t2.SubtractValue(t1), Throws.TypeOf<DataMisalignedException>()); 
    }

    [Test]
    public void SubstractValueCrossSerialNumberThrows()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt.Subtract(TimeSpan.FromMinutes(5)), 600, 0, Unit.Watt);
      var t2 = new TimeRegisterValue("2", dt, 600, 0, Unit.Watt);

      // Act & Assert
      Assert.That(() => t2.SubtractValue(t1), Throws.TypeOf<DataMisalignedException>()); 
    }

    [Test]
    public void SubstractValueDifferentUnitsThrows()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt, 20, 1, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt + TimeSpan.FromHours(1), 21, 1, Unit.WattHour);

      // Act & Assert
      Assert.That(() => t1.SubtractValue(t2), Throws.TypeOf<DataMisalignedException>()); 
    }
      
    [Test]
    public void ToStringTest()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);

      // Act
      var target = new TimeRegisterValue("1", dt, 10, 1, Unit.Watt);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("[serialNumber=1, timestamp=2015-02-13T19:30:00.0000000Z, unitValue=[value=100, unit=Watt]]"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt, 10, 1, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 10, 1, Unit.Watt);
      var t3 = new TimeRegisterValue("1", dt.AddSeconds(1), 10, 1, Unit.Watt);
      var t4 = new TimeRegisterValue("1", dt, 11, 1, Unit.Watt);
      var t5 = new TimeRegisterValue("1", dt, 10, 2, Unit.Watt);
      var t6 = new TimeRegisterValue("1", dt, 10, 1, Unit.WattHour);
      var t7 = new TimeRegisterValue();
      var t8 = new TimeRegisterValue();
      var t9 = new TimeRegisterValue("a", dt, 10, 1, Unit.Watt);
      var t10 = new TimeRegisterValue("A", dt, 10, 1, Unit.Watt);

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t5));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t6));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t6.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t7));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t7.GetHashCode()));
      Assert.That(t7, Is.EqualTo(t8));
      Assert.That(t7.GetHashCode(), Is.EqualTo(t8.GetHashCode()));
      Assert.That(t9, Is.EqualTo(t10));
      Assert.That(t9.GetHashCode(), Is.EqualTo(t10.GetHashCode()));
    }

    [Test]
    public void Equeality()
    {
        // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var t1 = new TimeRegisterValue("1", dt, 10, 1, Unit.Watt);
      var t2 = new TimeRegisterValue("1", dt, 10, 1, Unit.Watt);
      var t3 = new TimeRegisterValue("1", dt, 11, 1, Unit.Watt);

        // Act & Assert
        Assert.That(t1 == t2, Is.True);
        Assert.That(t1 == t3, Is.False);
    }

  }
}

