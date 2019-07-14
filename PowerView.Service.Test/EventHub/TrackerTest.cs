﻿using System;
using Autofac.Features.OwnedInstances;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class TrackerTest
  {
    private Mock<IFactory> factory;

    [SetUp]
    public void SetUp()
    {
      factory = new Mock<IFactory>();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var dateTime = DateTime.Now;

      // Act & Assert
      Assert.That(() => new Tracker(null, dateTime), Throws.ArgumentNullException);
      Assert.That(() => new Tracker(factory.Object, DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Track()
    {
      // Arrange
      var usageMonitor = new Mock<IUsageMonitor>();
      var disposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IUsageMonitor>()).Returns(new Owned<IUsageMonitor>(usageMonitor.Object, disposable.Object));
      var dateTime = new DateTime(2017, 8, 29, 19, 53, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime.Subtract(TimeSpan.FromHours(48)));

      // Act
      target.Track(dateTime);

      // Assert
      factory.Verify(f => f.Create<IUsageMonitor>());
      usageMonitor.Verify(um => um.TrackDing());
      disposable.Verify(x => x.Dispose());
    }

    [Test]
    public void DetectAndNotifyBeforeMinimumTimeSpan()
    {
      // Arrange
      var dateTime = new DateTime(2017, 8, 29, 19, 53, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);

      // Act
      target.Track(dateTime);

      // Assert
      factory.Verify(f => f.Create<IUsageMonitor>(), Times.Never());
    }

    private Tracker CreateTarget(DateTime dateTime)
    {
      return new Tracker(factory.Object, dateTime);
    }

  }
}