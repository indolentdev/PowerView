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
    private Mock<IFactory> factory;
    private Mock<IDbCheck> dbCheck;
    private Mock<IExitSignalProvider> exitSignalProvider;

    [SetUp]
    public void SetUp()
    {
      factory = new Mock<IFactory>();
      dbCheck = new Mock<IDbCheck>();
      exitSignalProvider = new Mock<IExitSignalProvider>();

      factory.Setup(f => f.Create<IDbCheck>()).Returns(() => new Owned<IDbCheck>(dbCheck.Object, new System.IO.MemoryStream()));
      factory.Setup(f => f.Create<IExitSignalProvider>()).Returns(() => new Owned<IExitSignalProvider>(exitSignalProvider.Object, new System.IO.MemoryStream()));
    }

    [Test]
    public void DailyCheck()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime - TimeSpan.FromDays(1));

      // Act
      target.DailyCheck(dateTime);

      // Assert
      factory.Verify(f => f.Create<IDbCheck>());
      dbCheck.Verify(dc => dc.CheckDatabase());
      factory.Verify(f => f.Create<IExitSignalProvider>(), Times.Never);
    }

    [Test]
    public void DailyCheckSignalsExitOnCorruptionError()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime - TimeSpan.FromDays(1));
      dbCheck.Setup(dc => dc.CheckDatabase()).Throws(new DataStoreCorruptException());

      // Act
      target.DailyCheck(dateTime);

      // Assert
      factory.Verify(f => f.Create<IDbCheck>());
      dbCheck.Verify(dc => dc.CheckDatabase());
      factory.Verify(f => f.Create<IExitSignalProvider>());
      exitSignalProvider.Verify(esp => esp.FireExitEvent());
    }

    [Test]
    public void DailyCheckSuccessiveBeforeInterval()
    {
      // Arrange
      var now = DateTime.Now;
      var target = CreateTarget(now);
      var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 14, 0, 0, now.Kind);

      // Act
      target.DailyCheck(dateTime+TimeSpan.FromDays(1));
      target.DailyCheck(dateTime+TimeSpan.FromDays(2));

      // Assert
      factory.Verify(f => f.Create<IDbCheck>(), Times.Once);
      dbCheck.Verify(dc => dc.CheckDatabase(), Times.Once);
    }

    [Test]
    public void DailyCheckSuccessiveAfterInterval()
    {
      // Arrange
      var now = DateTime.Now;
      var target = CreateTarget(now);
      var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 16, 0, 0, now.Kind);

      // Act
      target.DailyCheck(dateTime + TimeSpan.FromDays(1));
      target.DailyCheck(dateTime + TimeSpan.FromDays(2));

      // Assert
      factory.Verify(f => f.Create<IDbCheck>(), Times.Exactly(2));
      dbCheck.Verify(dc => dc.CheckDatabase(), Times.Exactly(2));
    }

    private HealthCheck CreateTarget(DateTime dateTime)
    {
      return new HealthCheck(factory.Object, dateTime);
    }
  }
}
