using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class NormalizedTimeRegisterValueTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var timeRegisterValue = new TimeRegisterValue();

      // Act & Assert
      Assert.That(() => new NormalizedTimeRegisterValue(timeRegisterValue, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var timeRegisterValue = new TimeRegisterValue();
      var dt = DateTime.UtcNow;

      // Act
      var target = new NormalizedTimeRegisterValue(timeRegisterValue, dt);

      // Assert
      Assert.That(target.TimeRegisterValue, Is.EqualTo(timeRegisterValue));
      Assert.That(target.NormalizedTimestamp, Is.EqualTo(dt));
      Assert.That(target.OrderProperty, Is.EqualTo(timeRegisterValue.Timestamp));
    }

    [Test]
    public void NormalizeThrows()
    {
      // Arrange
      var timeRegisterValue = new TimeRegisterValue();
      var dt = DateTime.UtcNow;
      var target = new NormalizedTimeRegisterValue(timeRegisterValue, dt);

      // Act & Assert
      Assert.That(() => target.Normalize(null), Throws.TypeOf<NotSupportedException>());
      Assert.That(() => target.Normalize(x => x), Throws.TypeOf<NotSupportedException>());
    }

    [Test]
    public void ToStringTest()
    {
      // Arrange
      var timeRegisterValue = new TimeRegisterValue();
      var dt = new DateTime(2019,04,18,15,27,11,DateTimeKind.Utc);

      // Act
      var target = new NormalizedTimeRegisterValue(timeRegisterValue, dt);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("[timeRegisterValue=[serialNumber=, timestamp=0001-01-01T00:00:00.0000000, unitValue=[value=0, unit=Watt]], normalizedTimestamp=2019-04-18T15:27:11.0000000Z]"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var timeRegisterValue1 = new TimeRegisterValue("SN1", DateTime.UtcNow, 11, Unit.CubicMetre);
      var timeRegisterValue2 = new TimeRegisterValue("SN2", DateTime.UtcNow.AddDays(1), 22, Unit.Joule);
      var dt1 = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var dt2 = dt1.AddDays(2);
      var t1 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
      var t2 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
      var t3 = new NormalizedTimeRegisterValue(timeRegisterValue2, dt1);
      var t4 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt2);

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
      var timeRegisterValue1 = new TimeRegisterValue("SN1", DateTime.UtcNow, 11, Unit.CubicMetre);
      var timeRegisterValue2 = new TimeRegisterValue("SN2", DateTime.UtcNow.AddDays(1), 22, Unit.Joule);
      var dt1 = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var t1 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
      var t2 = new NormalizedTimeRegisterValue(timeRegisterValue1, dt1);
      var t3 = new NormalizedTimeRegisterValue(timeRegisterValue2, dt1);

      // Act & Assert
      Assert.That(t1 == t2, Is.True);
      Assert.That(t1 == t3, Is.False);
    }

  }
}

