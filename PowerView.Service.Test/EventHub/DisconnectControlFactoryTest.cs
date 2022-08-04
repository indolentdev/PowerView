using System;
using Microsoft.Extensions.DependencyInjection;
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
    [Test]
    public void ProcessThrows()
    {
      // Arrange
      var target = CreateTarget();
      var (serviceScope, _) = GetServiceScope();
      var liveReadings = Array.Empty<LiveReading>();

      // Act & Assert
      Assert.That(() => target.Process(null, liveReadings), Throws.ArgumentNullException);
      Assert.That(() => target.Process(serviceScope.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    public void Process()
    {
      // Arrange
      var disconnectWarden = new Mock<IDisconnectWarden>();
      var (serviceScope, serviceProvider) = GetServiceScope();
      serviceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(disconnectWarden.Object);
      var liveReadings = new LiveReading[1];

      var target = CreateTarget();

      // Act
      target.Process(serviceScope.Object, liveReadings);

      // Assert
      serviceProvider.Verify(sp => sp.GetService(typeof(IDisconnectWarden)));
      disconnectWarden.Verify(p => p.Process(liveReadings));
    }

    [Test]
    public void ProcessEmptyLiveReadings()
    {
      // Arrange
      var liveReadings = new LiveReading[0];
      var (serviceScope, serviceProvider) = GetServiceScope();

      var target = CreateTarget();

      // Act
      target.Process(serviceScope.Object, liveReadings);

      // Assert
      serviceProvider.Verify(sp => sp.GetService(typeof(IDisconnectWarden)), Times.Never);
    }

    private DisconnectControlFactory CreateTarget()
    {
      return new DisconnectControlFactory();
    }

    private (Mock<IServiceScope> ServiceScope, Mock<IServiceProvider> ServiceProvider) GetServiceScope()
    {
      var serviceScope = new Mock<IServiceScope>();
      var serviceProvider = new Mock<IServiceProvider>();

      serviceScope.Setup(ss => ss.ServiceProvider).Returns(serviceProvider.Object);

      return (serviceScope, serviceProvider);
    }

  }
}
