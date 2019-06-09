using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class CoarseTimeRegisterValueTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var dt = DateTime.Now;
      var timeRegisterValue = new TimeRegisterValue();

      // Act & Assert
      Assert.That(() => new CoarseTimeRegisterValue(dt, timeRegisterValue), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var dt = DateTime.UtcNow.AddDays(-1);
      var timeRegisterValue = new TimeRegisterValue("1", DateTime.UtcNow, 100, 2, Unit.WattHour);

      // Act
      var target = new CoarseTimeRegisterValue(dt, timeRegisterValue);

      // Assert
      Assert.That(target.CoarseTimestamp, Is.EqualTo(dt));
      Assert.That(target.TimeRegisterValue, Is.EqualTo(timeRegisterValue));
    }

    [Test]
    public void ToStringTest()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,00,00,00,DateTimeKind.Utc);
      var timeRegisterValue = new TimeRegisterValue("1", new DateTime(2015, 02, 13, 19, 30, 51, DateTimeKind.Utc), 100, 0, Unit.Watt);

      // Act
      var target = new CoarseTimeRegisterValue(dt, timeRegisterValue);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("[coarseTimestamp=2015-02-13T00:00:00.0000000Z, timeRegisterValue=[serialNumber=1, timestamp=2015-02-13T19:30:51.0000000Z, unitValue=[value=100, unit=Watt]]]"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Utc);
      var timeRegisterValue = new TimeRegisterValue("1", new DateTime(2015, 02, 13, 19, 30, 51, DateTimeKind.Utc), 100, 0, Unit.Watt);
      var t1 = new CoarseTimeRegisterValue(dt, timeRegisterValue);
      var t2 = new CoarseTimeRegisterValue(dt, timeRegisterValue);
      var t3 = new CoarseTimeRegisterValue(dt.AddSeconds(1), timeRegisterValue);
      var t4 = new CoarseTimeRegisterValue(dt, new TimeRegisterValue());

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
    }

    [Test]
    public void Equeality()
    {
        // Arrange
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Utc);
      var timeRegisterValue = new TimeRegisterValue("1", new DateTime(2015, 02, 13, 19, 30, 51, DateTimeKind.Utc), 100, 0, Unit.Watt);
      var t1 = new CoarseTimeRegisterValue(dt, timeRegisterValue);
      var t2 = new CoarseTimeRegisterValue(dt, timeRegisterValue);
      var t3 = new CoarseTimeRegisterValue(dt.AddSeconds(1), timeRegisterValue);

        // Act & Assert
        Assert.That(t1 == t2, Is.True);
        Assert.That(t1 == t3, Is.False);
    }

  }
}
