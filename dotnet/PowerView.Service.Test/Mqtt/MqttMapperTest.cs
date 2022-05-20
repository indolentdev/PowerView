using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mqtt;
using MQTTnet.Protocol;

namespace PowerView.Service.Test.Mqtt
{
  [TestFixture]
  public class MqttMapperTest
  {
    [Test]
    public void Empty()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var mqttMsqs = target.Map(new LiveReading[0]);

      // Assert
      Assert.That(mqttMsqs, Is.Empty);
    }

    [Test]
    [TestCase("1.2.3.4.5.6")]
    public void UnsupportedObisCode(string obisCode)
    {
      // Arrange
      var target = CreateTarget();
      var liveReadings = GetLiveReading(obisCode: obisCode);

      // Act
      var mqttMsqs = target.Map(liveReadings);

      // Assert
      Assert.That(mqttMsqs, Is.Empty);
    }

    [Test]
    [TestCase("1.0.1.8.0.255", "theLabel", "Electricity/theLabel/Energy/Import")]
    [TestCase("1.0.1.7.0.255", "theLabel", "Electricity/theLabel/Power/Import")]
    [TestCase("1.0.21.7.0.255", "theLabel", "Electricity/theLabel/Power/Import/L1")]
    [TestCase("1.0.41.7.0.255", "theLabel", "Electricity/theLabel/Power/Import/L2")]
    [TestCase("1.0.61.7.0.255", "theLabel", "Electricity/theLabel/Power/Import/L3")]
    [TestCase("1.0.2.8.0.255", "theLabel", "Electricity/theLabel/Energy/Export")]
    [TestCase("1.0.2.7.0.255", "theLabel", "Electricity/theLabel/Power/Export")]
    [TestCase("1.0.22.7.0.255", "theLabel", "Electricity/theLabel/Power/Export/L1")]
    [TestCase("1.0.42.7.0.255", "theLabel", "Electricity/theLabel/Power/Export/L2")]
    [TestCase("1.0.62.7.0.255", "theLabel", "Electricity/theLabel/Power/Export/L3")]

    [TestCase("8.0.1.0.0.255", "theLabel", "ColdWater/theLabel/Volume/Import")]
    [TestCase("8.0.2.0.0.255", "theLabel", "ColdWater/theLabel/Flow/Import")]

    [TestCase("9.0.1.0.0.255", "theLabel", "HotWater/theLabel/Volume/Import")]
    [TestCase("9.0.2.0.0.255", "theLabel", "HotWater/theLabel/Flow/Import")]

    [TestCase("6.0.1.0.0.255", "theLabel", "Heat/theLabel/Energy")]
    [TestCase("6.0.8.0.0.255", "theLabel", "Heat/theLabel/Power")]
    [TestCase("6.0.2.0.0.255", "theLabel", "Heat/theLabel/Volume")]
    [TestCase("6.0.9.0.0.255", "theLabel", "Heat/theLabel/Flow")]
    [TestCase("6.0.10.0.0.255", "theLabel", "Heat/theLabel/Temperature/Inlet")]
    [TestCase("6.0.11.0.0.255", "theLabel", "Heat/theLabel/Temperature/Outlet")]
    public void ObisCodeAndUnitToTopic(string obisCode, string label, string topic)
    {
      // Arrange
      var target = CreateTarget();
      var liveReadings = GetLiveReading(obisCode: obisCode, label: label);

      // Act
      var mqttMsqs = target.Map(liveReadings);

      // Assert
      Assert.That(mqttMsqs.Length, Is.EqualTo(1));
      Assert.That(mqttMsqs[0].Topic, Is.EqualTo(topic));
    }

    [Test]
    public void LabelToTopic()
    {
      // Arrange
      var target = CreateTarget();
      var liveReadings = GetLiveReading("TheLabel");

      // Act
      var mqttMsqs = target.Map(liveReadings);

      // Assert
      Assert.That(mqttMsqs.Length, Is.EqualTo(1));
      Assert.That(mqttMsqs[0].Topic, Contains.Substring("TheLabel"));
    }

    [Test]
    [TestCase(4455, Unit.WattHour, -3, "kWh")]
    [TestCase(4455, Unit.Watt, 0, "W")]
    [TestCase(4455, Unit.CubicMetre, 0, "m3")]
    [TestCase(4455, Unit.CubicMetrePrHour, 0, "m3h")]
    [TestCase(4455, Unit.DegreeCelsius, 0, "C")]
    public void Payload(int value, Unit unit, int factor, string jsonUnit)
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = DateTime.UtcNow;
      var liveReadings = GetLiveReading(dateTime: dateTime, value: value, unit: unit);

      // Act
      var mqttMsqs = target.Map(liveReadings);

      // Assert
      Assert.That(mqttMsqs.Length, Is.EqualTo(1));
      const string payloadFormat = "{0}\"Value\":\"{1}\",\"Unit\":\"{2}\",\"Timestamp\":\"{3}\"{4}";
      var payload = string.Format(CultureInfo.InvariantCulture, payloadFormat, 
                                  "{", value * Math.Pow(10, factor), jsonUnit, dateTime.ToString("o"), "}");

      Assert.That(System.Text.Encoding.UTF8.GetString(mqttMsqs[0].Payload), Is.EqualTo(payload));
    }

    [Test]
    public void FixedProperties()
    {
      // Arrange
      var target = CreateTarget();
      var liveReadings = GetLiveReading();

      // Act
      var mqttMsqs = target.Map(liveReadings);

      // Assert
      Assert.That(mqttMsqs.Length, Is.EqualTo(1));
      Assert.That(mqttMsqs[0].QualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtMostOnce));
      Assert.That(mqttMsqs[0].Retain, Is.EqualTo(false));
    }

    private ICollection<LiveReading> GetLiveReading(string label = "lbl", ObisCode? obisCode = null, DateTime? dateTime = null, int value = 1, Unit unit = default(Unit))
    {
      if (dateTime == null)
      {
        dateTime = DateTime.UtcNow;
      }
      if (obisCode == null)
      {
        obisCode = ObisCode.ElectrActiveEnergyA14;
      }
      return new[] { new LiveReading(label, "SN", dateTime.Value, new[] { new RegisterValue(obisCode.Value, value, 0, unit) }) };
    }

    private MqttMapper CreateTarget()
    {
      return new MqttMapper();
    }
  }
}
