using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class MqttConfigTest
  {
    [Test]
    public void ConstructorDefaultsAndProperties()
    {
      // Arrange

      // Act
      var target = new MqttConfig(null, 1, true, null);

      // Assert
      Assert.That(target.Server, Is.EqualTo("localhost"));
      Assert.That(target.ClientId, Is.EqualTo("PowerView"));
      Assert.That(target.Timeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange

      // Act
      var target = new MqttConfig("theServer", 1234, true, "theClientId", TimeSpan.FromSeconds(2));

      // Assert
      Assert.That(target.Server, Is.EqualTo("theServer"));
      Assert.That(target.Port, Is.EqualTo(1234));
      Assert.That(target.PublishEnabled, Is.True);
      Assert.That(target.ClientId, Is.EqualTo("theClientId"));
      Assert.That(target.Timeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
    }

    [Test]
    public void ConstructorSettingsAndProperties()
    {
      // Arrange
      var settings = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("MQTT_Server", "TheServer"),
        new KeyValuePair<string, string>("MQTT_Port", "1234"),
        new KeyValuePair<string, string>("MQTT_PublishEnabled", "True"),
        new KeyValuePair<string, string>("MQTT_ClientId", "TheClientId"),
      };

      // Act
      var target = new MqttConfig(settings);

      // Assert
      Assert.That(target.Server, Is.EqualTo("TheServer"));
      Assert.That(target.Port, Is.EqualTo(1234));
      Assert.That(target.PublishEnabled, Is.True);
      Assert.That(target.ClientId, Is.EqualTo("TheClientId"));
      Assert.That(target.Timeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
    }

    [Test]
    public void ConstructorSettingsThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new MqttConfig(null), Throws.ArgumentNullException);
    }

    [Test]
    public void ConstructorSettingsDefaultsAndProperties()
    {
      // Arrange

      // Act
      var target = new MqttConfig(new List<KeyValuePair<string, string>>());

      // Assert
      Assert.That(target.Server, Is.EqualTo("localhost"));
      Assert.That(target.Port, Is.EqualTo(1883));
      Assert.That(target.PublishEnabled, Is.False);
      Assert.That(target.ClientId, Is.EqualTo("PowerView"));
      Assert.That(target.Timeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
    }

    [Test]
    public void GetSettings()
    {
      // Arrange
      var target = new MqttConfig("theServer", 1234, true, "theClientId");

      // Act
      var settings = target.GetSettings();

      // Assert
      Assert.That(settings.Count, Is.EqualTo(4));
      Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("MQTT_Server", "theServer")));
      Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("MQTT_Port", "1234")));
      Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("MQTT_PublishEnabled", "True")));
      Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("MQTT_ClientId", "theClientId")));
    }
  }
}
