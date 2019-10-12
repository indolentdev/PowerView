using System;
using Autofac.Features.OwnedInstances;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class HealthCheckTest
  {
    private Mock<IIntervalTrigger> intervalTrigger;
    private Mock<IFactory> factory;
    private Mock<IDbCheck> dbCheck;
    private Mock<IExitSignalProvider> exitSignalProvider;

    [SetUp]
    public void SetUp()
    {
      intervalTrigger = new Mock<IIntervalTrigger>();
      factory = new Mock<IFactory>();
      dbCheck = new Mock<IDbCheck>();
      exitSignalProvider = new Mock<IExitSignalProvider>();

      factory.Setup(f => f.Create<IDbCheck>()).Returns(() => new Owned<IDbCheck>(dbCheck.Object, new System.IO.MemoryStream()));
      factory.Setup(f => f.Create<IExitSignalProvider>()).Returns(() => new Owned<IExitSignalProvider>(exitSignalProvider.Object, new System.IO.MemoryStream()));
    }

    [Test]
    public void Constructor()
    {
      // Arrange

      // Act
      var target = CreateTarget();

      // Assert
      intervalTrigger.Verify(it => it.Setup(TimeSpan.FromMinutes(15), TimeSpan.FromDays(1)));
    }


    [Test]
    public void DailyCheck()
    {
      // Arrange
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.DailyCheck(dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IDbCheck>());
      dbCheck.Verify(dc => dc.CheckDatabase());
      factory.Verify(f => f.Create<IExitSignalProvider>(), Times.Never);
      intervalTrigger.Verify(it => it.Advance(dateTime));
    }

    [Test]
    public void DailyCheckSignalsExitOnCorruptionError()
    {
      // Arrange
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      dbCheck.Setup(dc => dc.CheckDatabase()).Throws(new DataStoreCorruptException());

      // Act
      target.DailyCheck(dateTime);

      // Assert
      dbCheck.Verify(dc => dc.CheckDatabase());
      factory.Verify(f => f.Create<IExitSignalProvider>());
      exitSignalProvider.Verify(esp => esp.FireExitEvent());
    }

    private HealthCheck CreateTarget()
    {
      return new HealthCheck(intervalTrigger.Object, factory.Object);
    }
  }
}
