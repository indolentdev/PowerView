using System;
using Moq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DisconnectCacheItemTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new DisconnectCacheItem(null), Throws.ArgumentNullException);
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var rule = new Mock<IDisconnectRule>();

      // Act
      var target = new DisconnectCacheItem(rule.Object);

      // Assert
      Assert.That(target.Rule, Is.SameAs(rule.Object));
      Assert.That(target.Connected, Is.False);
      Assert.That(target.Count, Is.Zero);
    }

    [Test]
    public void AddOne()
    {
      // Arrange
      var rule = new Mock<IDisconnectRule>();
      var trv1 = new TimeRegisterValue();
      var target = new DisconnectCacheItem(rule.Object);

      // Act
      target.Add(new[] { trv1 });

      // Assert
      Assert.That(target.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddDuplicate()
    {
      // Arrange
      var rule = new Mock<IDisconnectRule>();
      var trv1 = new TimeRegisterValue();
      var trv2 = new TimeRegisterValue();
      var target = new DisconnectCacheItem(rule.Object);

      // Act
      target.Add(new[] { trv1, trv2 });

      // Assert
      Assert.That(target.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddTwo()
    {
      // Arrange
      var rule = new Mock<IDisconnectRule>();
      var trv1 = new TimeRegisterValue();
      var trv2 = new TimeRegisterValue("sn", DateTime.UtcNow, 1, Unit.Watt);
      var target = new DisconnectCacheItem(rule.Object);

      // Act
      target.Add(new[] { trv1, trv2 });

      // Assert
      Assert.That(target.Count, Is.EqualTo(2));
    }

    [Test]
    public void CalculateThrows()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());

      // Act & Assert
      Assert.That(() => target.Calculate(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void CalculateDisconnectedToConnected()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());

      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1400, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(30));

      // Assert
      Assert.That(target.Connected, Is.True);
    }

    [Test]
    public void CalculateConnectedToDisconnected()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());
      SetConnected(target);

      var time = new DateTime(2018, 8, 11, 16, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 200, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 550, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(15));

      // Assert
      Assert.That(target.Connected, Is.False);
    }

    [Test]
    public void CalculateCleansValuesByObsoleteTimeStamp()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());

      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1400, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(60));

      // Assert
      Assert.That(target.Count, Is.Zero);
    }

    [Test]
    public void CalculateCleansValuesByWrongUnit()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());

      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Joule),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1400, 0, Unit.CubicMetrePrHour),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(30));

      // Assert
      Assert.That(target.Count, Is.Zero);
    }

    [Test]
    public void CalculateInsufficientReadingDurationCoverage()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());
      SetConnected(target);

      var time = new DateTime(2018, 8, 11, 16, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(19), 1400, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(30));

      // Assert
      Assert.That(target.Connected, Is.False);
    }

    [Test]
    public void CalculateInsufficientReadingAverageValue()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());
      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 1500, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1498, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(30));

      // Assert
      Assert.That(target.Connected, Is.False);
    }

    [Test]
    public void CalculateIdempotent()
    {
      // Arrange
      var target = new DisconnectCacheItem(GetRule());
      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1400, 0, Unit.Watt),
      });

      // Act
      target.Calculate(time + TimeSpan.FromMinutes(30));
      target.Calculate(time + TimeSpan.FromMinutes(30));

      // Assert
      Assert.That(target.Connected, Is.True);
    }

    private void SetConnected(DisconnectCacheItem target)
    {
      var time = new DateTime(2018, 8, 11, 15, 0, 0, DateTimeKind.Utc);
      target.Add(new[] { new TimeRegisterValue("0", time + TimeSpan.FromMinutes(0), 2000, 0, Unit.Watt),
        new TimeRegisterValue("0", time + TimeSpan.FromMinutes(20), 1400, 0, Unit.Watt),
      });
      target.Calculate(time + TimeSpan.FromMinutes(30));
      Assert.That(target.Connected, Is.True);
    }

    private IDisconnectRule GetRule()
    {
      var rule = new Mock<IDisconnectRule>();
      rule.Setup(r => r.Name).Returns(new SerieName("Relay", "0.1.96.3.10.255"));
      rule.Setup(r => r.EvaluationName).Returns(new SerieName("Meter", ObisCode.ActualPowerP14L1));
      rule.Setup(r => r.Duration).Returns(TimeSpan.FromMinutes(30));
      rule.Setup(r => r.ConnectToDisconnectValue).Returns(500);
      rule.Setup(r => r.DisconnectToConnectValue).Returns(1500);
      rule.Setup(r => r.Unit).Returns(Unit.Watt);
      return rule.Object;
    }

  }
}
