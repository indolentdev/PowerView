using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class PeriodRegisterValueTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var dt = DateTime.UtcNow;
      var unitValue = new UnitValue();

      // Act & Assert
      Assert.That(() => new PeriodRegisterValue(DateTime.Now, dt, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new PeriodRegisterValue(dt, DateTime.Now, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new PeriodRegisterValue(dt, dt.Subtract(TimeSpan.FromMilliseconds(1)), unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var dtStart = DateTime.UtcNow;
      var dtEnd = dtStart.AddHours(1);
      var unitValue = new UnitValue(1, 2, Unit.Joule);

      // Act
      var target = new PeriodRegisterValue(dtStart, dtEnd, unitValue);

      // Assert
      Assert.That(target.StartTimestamp, Is.EqualTo(dtStart));
      Assert.That(target.EndTimestamp, Is.EqualTo(dtEnd));
      Assert.That(target.UnitValue, Is.EqualTo(unitValue));
    }

    [Test]
    public void ToStringTest()
    {
      // Arrange
      var dtStart = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var dtEnd = dtStart.AddHours(1);
      var unitValue = new UnitValue(1, 2, Unit.Joule);

      // Act
      var target = new PeriodRegisterValue(dtStart, dtEnd, unitValue);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("[startTimestamp=2015-02-13T19:30:00.0000000Z, endTimestamp=2015-02-13T20:30:00.0000000Z, unitValue=[value=100, unit=Joule]]"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var dtStart = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var dtEnd = new DateTime(2015, 02, 13, 20, 30, 00, DateTimeKind.Utc);
      var unitValue = new UnitValue(1, 2, Unit.Joule);
      var t1 = new PeriodRegisterValue(dtStart, dtEnd, unitValue);
      var t2 = new PeriodRegisterValue(dtStart, dtEnd, unitValue);
      var t3 = new PeriodRegisterValue(dtStart.AddMinutes(1), dtEnd, unitValue);
      var t4 = new PeriodRegisterValue(dtStart, dtEnd.AddMinutes(1), unitValue);
      var t5 = new PeriodRegisterValue(dtStart, dtEnd, new UnitValue());

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t5));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));
    }

    [Test]
    public void Equeality()
    {
      // Arrange
      var dtStart = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var dtEnd = new DateTime(2015, 02, 13, 20, 30, 00, DateTimeKind.Utc);
      var unitValue = new UnitValue(1, 2, Unit.Joule);
      var t1 = new PeriodRegisterValue(dtStart, dtEnd, unitValue);
      var t2 = new PeriodRegisterValue(dtStart, dtEnd, unitValue);
      var t3 = new PeriodRegisterValue(dtStart.AddMinutes(1), dtEnd, unitValue);

      // Act & Assert
      Assert.That(t1 == t2, Is.True);
      Assert.That(t1 == t3, Is.False);
    }

  }
}

