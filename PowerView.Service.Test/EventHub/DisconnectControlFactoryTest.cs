using System;
using Autofac.Features.OwnedInstances;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;
using PowerView.Service.DisconnectControl;
using PowerView.Model;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class DisconnectControlFactoryTest
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

      // Act & Assert
      Assert.That(() => new DisconnectControlFactory(null), Throws.ArgumentNullException);
    }

    [Test]
    public void ProcessThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Process(null), Throws.ArgumentNullException);
    }

    [Test]
    public void Process()
    {
      // Arrange
      var disconnectCalculator = new Mock<IDisconnectWarden>();
      var mDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IDisconnectWarden>()).Returns(new Owned<IDisconnectWarden>(disconnectCalculator.Object, mDisposable.Object));

      var liveReadings = new LiveReading[1];

      var target = CreateTarget();

      // Act
      target.Process(liveReadings);

      // Assert
      factory.Verify(f => f.Create<IDisconnectWarden>());
      disconnectCalculator.Verify(p => p.Process(liveReadings));
      mDisposable.Verify(x => x.Dispose());
    }

    [Test]
    public void ProcessEmptyLiveReadings()
    {
      // Arrange
      var liveReadings = new LiveReading[0];

      var target = CreateTarget();

      // Act
      target.Process(liveReadings);

      // Assert
      factory.Verify(f => f.Create<IDisconnectWarden>(), Times.Never());
    }

    private DisconnectControlFactory CreateTarget()
    {
      return new DisconnectControlFactory(factory.Object);
    }

  }
}
