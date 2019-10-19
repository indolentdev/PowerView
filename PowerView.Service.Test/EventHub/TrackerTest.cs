using System;
using Autofac.Features.OwnedInstances;
using Moq;
using NUnit.Framework;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class TrackerTest
  {
    private Mock<IIntervalTrigger> intervalTrigger;
    private Mock<IFactory> factory;

    [SetUp]
    public void SetUp()
    {
      intervalTrigger = new Mock<IIntervalTrigger>();
      factory = new Mock<IFactory>();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new Tracker(null, factory.Object), Throws.ArgumentNullException);
      Assert.That(() => new Tracker(null, factory.Object), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor()
    {
      // Arrange

      // Act
      var target = CreateTarget();

      // Assert
      intervalTrigger.Verify(it => it.Setup(TimeSpan.FromHours(12), TimeSpan.FromDays(1)));
    }


    [Test]
    public void Track()
    {
      // Arrange
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      var envRepository = new Mock<IEnvironmentRepository>();
      var disposable1 = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IEnvironmentRepository>()).Returns(new Owned<IEnvironmentRepository>(envRepository.Object, disposable1.Object));
      const string sqliteVersion = "TheVersion";
      envRepository.Setup(x => x.GetSqliteVersion()).Returns(sqliteVersion);
      var usageMonitor = new Mock<IUsageMonitor>();
      var disposable2 = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IUsageMonitor>()).Returns(new Owned<IUsageMonitor>(usageMonitor.Object, disposable2.Object));
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      target.Track(dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      intervalTrigger.Verify(it => it.Advance(dateTime));
      factory.Verify(f => f.Create<IEnvironmentRepository>());
      envRepository.Verify(x => x.GetSqliteVersion());
      disposable1.Verify(x => x.Dispose());
      factory.Verify(f => f.Create<IUsageMonitor>());
      usageMonitor.Verify(um => um.TrackDing(It.Is<string>(x => x == sqliteVersion), It.IsAny<string>()));
      disposable2.Verify(x => x.Dispose());
    }

    [Test]
    public void TrackNoTrigger()
    {
      // Arrange
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      target.Track(dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IUsageMonitor>(), Times.Never());
      intervalTrigger.Verify(it => it.Advance(It.IsAny<DateTime>()), Times.Never);
    }

    private Tracker CreateTarget()
    {
      return new Tracker(intervalTrigger.Object, factory.Object);
    }

  }
}
