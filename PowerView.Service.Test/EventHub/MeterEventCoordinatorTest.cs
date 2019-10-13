using System;
using Autofac.Features.OwnedInstances;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class MeterEventCoordinatorTest
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
      Assert.That(() => new MeterEventCoordinator(null, factory.Object), Throws.ArgumentNullException);
      Assert.That(() => new MeterEventCoordinator(intervalTrigger.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor()
    {
      // Arrange

      // Act
      var target = CreateTarget();

      // Assert
      intervalTrigger.Verify(it => it.Setup(new TimeSpan(6, 0, 0), TimeSpan.FromDays(1)));
    }

    [Test]
    public void DetectAndNotify()
    {
      // Arrange
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      var meterEventDetector = new Mock<IMeterEventDetector>();
      var medDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IMeterEventDetector>()).Returns(new Owned<IMeterEventDetector>(meterEventDetector.Object, medDisposable.Object));
      var meterEventNotifier = new Mock<IMeterEventNotifier>();
      var menDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IMeterEventNotifier>()).Returns(new Owned<IMeterEventNotifier>(meterEventNotifier.Object, menDisposable.Object));
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      target.DetectAndNotify(dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IMeterEventDetector>());
      meterEventDetector.Verify(med => med.DetectMeterEvents(It.Is<DateTime>(x => x == dateTime.Date.ToUniversalTime() && x.Kind == DateTimeKind.Utc)));
      medDisposable.Verify(x => x.Dispose());
      factory.Verify(f => f.Create<IMeterEventNotifier>());
      meterEventNotifier.Verify(men => men.NotifyEmailRecipients());
      menDisposable.Verify(x => x.Dispose());
      intervalTrigger.Verify(it => it.Advance(dateTime));
    }

    [Test]
    public void DetectAndNotifyNoTrigger()
    {
      // Arrange
      intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      target.DetectAndNotify(dateTime);

      // Assert
      intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IMeterEventDetector>(), Times.Never);
      intervalTrigger.Verify(it => it.Advance(dateTime), Times.Never);
    }

    private MeterEventCoordinator CreateTarget()
    {
      return new MeterEventCoordinator(intervalTrigger.Object, factory.Object);
    }

  }
}
