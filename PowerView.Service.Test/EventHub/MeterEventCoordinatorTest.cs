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
      Assert.That(() => new MeterEventCoordinator(null, dateTime), Throws.ArgumentNullException);
      Assert.That(() => new MeterEventCoordinator(factory.Object, DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void DetectAndNotify()
    {
      // Arrange
      var meterEventDetector = new Mock<IMeterEventDetector>();
      var medDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IMeterEventDetector>()).Returns(new Owned<IMeterEventDetector>(meterEventDetector.Object, medDisposable.Object));
      var meterEventNotifier = new Mock<IMeterEventNotifier>();
      var menDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IMeterEventNotifier>()).Returns(new Owned<IMeterEventNotifier>(meterEventNotifier.Object, menDisposable.Object));
      var dateTime = new DateTime(2017, 8, 29, 19, 53, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime.Subtract(TimeSpan.FromHours(48)));

      // Act
      target.DetectAndNotify(dateTime);

      // Assert
      factory.Verify(f => f.Create<IMeterEventDetector>());
      meterEventDetector.Verify(med => med.DetectMeterEvents(It.Is<DateTime>(x => x == dateTime.ToUniversalTime() && x.Kind == DateTimeKind.Utc)));
      medDisposable.Verify(x => x.Dispose());
      factory.Verify(f => f.Create<IMeterEventNotifier>());
      meterEventNotifier.Verify(men => men.NotifyEmailRecipients());
      menDisposable.Verify(x => x.Dispose());
    }

    [Test]
    public void DetectAndNotifyBeforeMinimumTimeSpan()
    {
      // Arrange
      var dateTime = new DateTime(2017, 8, 29, 19, 53, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);

      // Act
      target.DetectAndNotify(dateTime);

      // Assert
      factory.Verify(f => f.Create<IMeterEventDetector>(), Times.Never());
      factory.Verify(f => f.Create<IMeterEventNotifier>(), Times.Never());
    }

    private MeterEventCoordinator CreateTarget(DateTime dateTime)
    {
      return new MeterEventCoordinator(factory.Object, dateTime);
    }

  }
}
