using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;
using PowerView.Service.Mqtt;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class MqttPublisherFactoryTest
  {
    private Mock<IServiceScope> serviceScope;
    private Mock<IServiceProvider> serviceProvider;

    [SetUp]
    public void SetUp()
    {
      serviceScope = new Mock<IServiceScope>();
      serviceProvider = new Mock<IServiceProvider>();
      serviceScope.Setup(ss => ss.ServiceProvider).Returns(serviceProvider.Object);
    }
    
    [Test]
    public void PublishThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Publish(serviceScope.Object, null), Throws.ArgumentNullException);
      Assert.That(() => target.Publish(null, Array.Empty<Reading>()), Throws.ArgumentNullException);
    }

    [Test]
    public void Publish()
    {
      // Arrange
      var settingsRepository = new Mock<ISettingRepository>();
      serviceProvider.Setup(sp => sp.GetService(typeof(ISettingRepository))).Returns(settingsRepository.Object);

      var mqttConfig = new MqttConfig("theServer", 1234, true, "theClientId");
      settingsRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

      var mqttPublisher = new Mock<IMqttPublisher>();
      serviceProvider.Setup(sp => sp.GetService(typeof(IMqttPublisher))).Returns(mqttPublisher.Object);

      var liveReadings = new Reading[1];

      var target = CreateTarget();

      // Act
      target.Publish(serviceScope.Object, liveReadings);

      // Assert
      serviceProvider.Verify(sp => sp.GetService(typeof(ISettingRepository)));
      settingsRepository.Verify(r => r.GetMqttConfig());
      serviceProvider.Verify(sp => sp.GetService(typeof(IMqttPublisher)));
      mqttPublisher.Verify(p => p.Publish(mqttConfig, liveReadings));
    }

    [Test]
    public void PublishEmptyLiveReadings()
    {
      // Arrange
      var liveReadings = new Reading[0];

      var target = CreateTarget();

      // Act
      target.Publish(serviceScope.Object, liveReadings);

      // Assert
      serviceProvider.Verify(sp => sp.GetService(typeof(ISettingRepository)), Times.Never);
      serviceProvider.Verify(sp => sp.GetService(typeof(IMqttPublisher)), Times.Never);
    }

    [Test]
    public void PublishMqttDisabled()
    {
      // Arrange
      var settingsRepository = new Mock<ISettingRepository>();
      serviceProvider.Setup(sp => sp.GetService(typeof(ISettingRepository))).Returns(settingsRepository.Object);

      var mqttConfig = new MqttConfig("theServer", 1234, false, "theClientId");
      settingsRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

      var liveReadings = new Reading[1];

      var target = CreateTarget();

      // Act
      target.Publish(serviceScope.Object, liveReadings);

      // Assert
      serviceProvider.Verify(sp => sp.GetService(typeof(ISettingRepository)));
      settingsRepository.Verify(r => r.GetMqttConfig());
      serviceProvider.Verify(sp => sp.GetService(typeof(IMqttPublisher)), Times.Never);
    }

    private MqttPublisherFactory CreateTarget()
    {
      return new MqttPublisherFactory();
    }

  }
}
