﻿using System.Collections.Generic;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mqtt;
using Moq;
using MQTTnet;

namespace PowerView.Service.Test.Mqtt
{
  [TestFixture]
  public class MqttPublisherTest
  {
//    [OneTimeSetUp]
//    public void OneTimeSetUp()
//    {
//      log4net.Config.BasicConfigurator.Configure();
//    }

    private TestMqttServer mqttServer;
    private Mock<IMqttMapper> mqttMapper;

    [SetUp]
    public void SetUp()
    {
      mqttServer = new TestMqttServer();
      mqttMapper = new Mock<IMqttMapper>();
    }

    [TearDown]
    public void TearDown()
    {
      mqttServer.Dispose();
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new MqttPublisher(null), Throws.ArgumentNullException);
    }

    [Test]
    public void PublishThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Publish(null, new LiveReading[0]), Throws.ArgumentNullException);
      Assert.That(() => target.Publish(mqttServer.GetClientConfig(), null), Throws.ArgumentNullException);
    }

    [Test]
    public void MqttServerUnavailable()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Publish(mqttServer.GetClientConfig(), new LiveReading[1]), Throws.TypeOf<ConnectMqttException>());
    }

    [Test]
    public void PublishLiveReadingEmpty()
    {
      // Arrange
      mqttServer.Start();
      var target = CreateTarget();
      var liveReadings = new LiveReading[0];
      mqttMapper.Setup(mm => mm.Map(It.IsAny<ICollection<LiveReading>>())).Returns(new MqttApplicationMessage[0]);

      // Act
      target.Publish(mqttServer.GetClientConfig(), liveReadings);

      // Assert
      mqttServer.AssertPublishCount(0);
    }

    [Test]
    public void PublishOneMessage()
    {
      // Arrange
      mqttServer.Start();
      var target = CreateTarget();
      var liveReadings = new LiveReading[1];
      var mqttMessage = new MqttApplicationMessageBuilder().WithTopic("TheTopic").WithAtMostOnceQoS().Build();
      mqttMapper.Setup(mm => mm.Map(It.IsAny<ICollection<LiveReading>>())).Returns(new[] { mqttMessage });

      // Act
      target.Publish(mqttServer.GetClientConfig(), liveReadings);

      // Assert
      System.Threading.Thread.Sleep(40); // Nasty sleep as MQTTnet lib behaves iffy..
      mqttServer.AssertPublishCount(1);
      Assert.That(mqttServer.Published[0].ClientId, Is.Empty);
      Assert.That(mqttServer.Published[0].ApplicationMessage.Topic, Is.EqualTo(mqttMessage.Topic));
      mqttMapper.Verify(mm => mm.Map(liveReadings));
    }

    private MqttPublisher CreateTarget()
    {
      return new MqttPublisher(mqttMapper.Object);
    }

  }
}
