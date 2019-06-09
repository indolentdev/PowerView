using System;
using Autofac.Features.OwnedInstances;
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
      Assert.That(() => new MqttPublisherFactory(null), Throws.ArgumentNullException);
    }

    [Test]
    public void PublishThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Publish(null), Throws.ArgumentNullException);
    }

    [Test]
    public void Publish()
    {
      // Arrange
      var settingsRepository = new Mock<ISettingRepository>();
      var repoDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<ISettingRepository>()).Returns(new Owned<ISettingRepository>(settingsRepository.Object, repoDisposable.Object));

      var mqttConfig = new MqttConfig("theServer", 1234, true);
      settingsRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

      var mqttPublisher = new Mock<IMqttPublisher>();
      var mDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<IMqttPublisher>()).Returns(new Owned<IMqttPublisher>(mqttPublisher.Object, mDisposable.Object));

      var liveReadings = new LiveReading[1];

      var target = CreateTarget();

      // Act
      target.Publish(liveReadings);

      // Assert
      factory.Verify(f => f.Create<ISettingRepository>());
      settingsRepository.Verify(r => r.GetMqttConfig());
      repoDisposable.Verify(x => x.Dispose());
      factory.Verify(f => f.Create<IMqttPublisher>());
      mqttPublisher.Verify(p => p.Publish(mqttConfig, liveReadings));
      mDisposable.Verify(x => x.Dispose());
    }

    [Test]
    public void PublishEmptyLiveReadings()
    {
      // Arrange
      var liveReadings = new LiveReading[0];

      var target = CreateTarget();

      // Act
      target.Publish(liveReadings);

      // Assert
      factory.Verify(f => f.Create<ISettingRepository>(), Times.Never());
      factory.Verify(f => f.Create<IMqttPublisher>(), Times.Never());
    }

    [Test]
    public void PublishMqttDisabled()
    {
      // Arrange
      var settingsRepository = new Mock<ISettingRepository>();
      var repoDisposable = new Mock<IDisposable>();
      factory.Setup(f => f.Create<ISettingRepository>()).Returns(new Owned<ISettingRepository>(settingsRepository.Object, repoDisposable.Object));

      var mqttConfig = new MqttConfig("theServer", 1234, false);
      settingsRepository.Setup(sr => sr.GetMqttConfig()).Returns(mqttConfig);

      var liveReadings = new LiveReading[1];

      var target = CreateTarget();

      // Act
      target.Publish(liveReadings);

      // Assert
      factory.Verify(f => f.Create<ISettingRepository>());
      settingsRepository.Verify(r => r.GetMqttConfig());
      repoDisposable.Verify(x => x.Dispose());
      factory.Verify(f => f.Create<IMqttPublisher>(), Times.Never());
    }

    private MqttPublisherFactory CreateTarget()
    {
      return new MqttPublisherFactory(factory.Object);
    }

  }
}
