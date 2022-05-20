using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class HealthCheckTest
  {
    private Mock<IIntervalTrigger> intervalTrigger;
    private Mock<IServiceScope> serviceScope;
    private Mock<IServiceProvider> serviceProvider;
    private Mock<IDbCheck> dbCheck;
    private Mock<IExitSignalProvider> exitSignalProvider;

    [SetUp]
    public void SetUp()
    {
      intervalTrigger = new Mock<IIntervalTrigger>();
      serviceScope = new Mock<IServiceScope>();
      serviceProvider = new Mock<IServiceProvider>();
      serviceScope.Setup(ss => ss.ServiceProvider).Returns(serviceProvider.Object);

      dbCheck = new Mock<IDbCheck>();
      exitSignalProvider = new Mock<IExitSignalProvider>();

      serviceProvider.Setup(sp => sp.GetService(typeof(IDbCheck))).Returns(dbCheck.Object);
      serviceProvider.Setup(sp => sp.GetService(typeof(IExitSignalProvider))).Returns(exitSignalProvider.Object);
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
      target.DailyCheck(serviceScope.Object, dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      serviceProvider.Verify(sp => sp.GetService(typeof(IDbCheck)));
      dbCheck.Verify(dc => dc.CheckDatabase());
      serviceProvider.Verify(sp => sp.GetService(typeof(IExitSignalProvider)), Times.Never);
      intervalTrigger.Verify(it => it.Advance(dateTime));
    }

    [Test]
    public void DailyCheckNoTrigger()
    {
      // Arrange
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);

      // Act
      target.DailyCheck(serviceScope.Object, dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      serviceProvider.Verify(sp => sp.GetService(typeof(IDbCheck)), Times.Never);
      intervalTrigger.Verify(it => it.Advance(dateTime), Times.Never);
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
      target.DailyCheck(serviceScope.Object, dateTime);

      // Assert
      dbCheck.Verify(dc => dc.CheckDatabase());
      serviceProvider.Verify(sp => sp.GetService(typeof(IExitSignalProvider)));
      exitSignalProvider.Verify(esp => esp.FireExitEvent());
    }

    private HealthCheck CreateTarget()
    {
      return new HealthCheck(new NullLogger<HealthCheck>(), intervalTrigger.Object);
    }
  }
}
