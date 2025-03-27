using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PowerView.Model;
using MQTTnet;

namespace PowerView.Service.Mqtt
{
  public class MqttMapper : IMqttMapper
  {
    public MqttApplicationMessage[] Map(ICollection<Reading> liveReadings)
    {
      var pubs = new List<MqttApplicationMessage>();
      foreach (var pubItem in GetPublishItems(liveReadings))
      {
        var topic = GetTopic(pubItem);
        if (string.IsNullOrEmpty(topic)) continue;

        var payload = GetPayload(pubItem);
        if (string.IsNullOrEmpty(payload)) continue;

        var msg = new MqttApplicationMessageBuilder()
          .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
          .WithRetainFlag(false)
          .WithTopic(topic)
          .WithPayload(payload)
          .Build();        
        pubs.Add(msg);
      }
      return pubs.ToArray();
    }

    private static IEnumerable<PublishItem> GetPublishItems(ICollection<Reading> liveReadings)
    {
      foreach (var liveReading in liveReadings)
      {
        foreach (var registerValue in liveReading.GetRegisterValues())
        {
          yield return new PublishItem
          {
            Label = liveReading.Label,
            ObisCode = registerValue.ObisCode,
            TimeRegisterValue = new TimeRegisterValue(liveReading.DeviceId, liveReading.Timestamp, registerValue.Value,
                                             registerValue.Scale, registerValue.Unit)
          };
        }
      }
    }

    private class PublishItem
    {
      public string Label { get; set; }
      public ObisCode ObisCode { get; set; }
      public TimeRegisterValue TimeRegisterValue { get; set; }
    }

    private static string GetTopic(PublishItem pubItem)
    {
      var topicFormat = ObisCodeToTopicFormat(pubItem.ObisCode);
      if (topicFormat == null) return null;

      return string.Format(CultureInfo.InvariantCulture, topicFormat, pubItem.Label);
    }

    private static string ObisCodeToTopicFormat(ObisCode obisCode)
    {
      if (obisCode == ObisCode.ElectrActiveEnergyA14) return "Electricity/{0}/Energy/Import";
      if (obisCode == ObisCode.ElectrActualPowerP14) return "Electricity/{0}/Power/Import";
      if (obisCode == ObisCode.ElectrActualPowerP14L1) return "Electricity/{0}/Power/Import/L1";
      if (obisCode == ObisCode.ElectrActualPowerP14L2) return "Electricity/{0}/Power/Import/L2";
      if (obisCode == ObisCode.ElectrActualPowerP14L3) return "Electricity/{0}/Power/Import/L3";
      if (obisCode == ObisCode.ElectrActiveEnergyA23) return "Electricity/{0}/Energy/Export";
      if (obisCode == ObisCode.ElectrActualPowerP23) return "Electricity/{0}/Power/Export";
      if (obisCode == ObisCode.ElectrActualPowerP23L1) return "Electricity/{0}/Power/Export/L1";
      if (obisCode == ObisCode.ElectrActualPowerP23L2) return "Electricity/{0}/Power/Export/L2";
      if (obisCode == ObisCode.ElectrActualPowerP23L3) return "Electricity/{0}/Power/Export/L3";

      if (obisCode == ObisCode.ColdWaterVolume1) return "ColdWater/{0}/Volume/Import";
      if (obisCode == ObisCode.ColdWaterFlow1) return "ColdWater/{0}/Flow/Import";

      if (obisCode == ObisCode.HotWaterVolume1) return "HotWater/{0}/Volume/Import";
      if (obisCode == ObisCode.HotWaterFlow1) return "HotWater/{0}/Flow/Import";

      if (obisCode == ObisCode.HeatEnergyEnergy1) return "Heat/{0}/Energy";
      if (obisCode == ObisCode.HeatEnergyPower1) return "Heat/{0}/Power";
      if (obisCode == ObisCode.HeatEnergyVolume1) return "Heat/{0}/Volume";
      if (obisCode == ObisCode.HeatEnergyFlow1) return "Heat/{0}/Flow";
      if (obisCode == ObisCode.HeatEnergyFlowTemperature) return "Heat/{0}/Temperature/Inlet";
      if (obisCode == ObisCode.HeatEnergyReturnTemperature) return "Heat/{0}/Temperature/Outlet";

      return null;
    }

    private static UnitAndFactor GetUnitAndFactor(Unit unit)
    {
      switch (unit)
      {
        case Unit.WattHour: return new UnitAndFactor { Unit = "kWh", Factor = -3 };
        case Unit.Watt: return new UnitAndFactor { Unit = "W", Factor = 0 };
        case Unit.CubicMetre: return new UnitAndFactor { Unit = "m3", Factor = 0 };
        case Unit.CubicMetrePrHour: return new UnitAndFactor { Unit = "m3h", Factor = 0 };
        case Unit.DegreeCelsius: return new UnitAndFactor { Unit = "C", Factor = 0 };
        default: return null;
      }
    }

    private class UnitAndFactor
    {
      public string Unit { get; set; }
      public int Factor { get; set; }
    }

    private static string GetPayload(PublishItem pubItem)
    {
      var unitAndFactor = GetUnitAndFactor(pubItem.TimeRegisterValue.UnitValue.Unit);
      if (unitAndFactor == null) return null;

      double adjustedValue = pubItem.TimeRegisterValue.UnitValue.Value * Math.Pow(10, unitAndFactor.Factor);

      var payload = new { Value = adjustedValue.ToString(CultureInfo.InvariantCulture), 
        Unit = unitAndFactor.Unit, Timestamp = pubItem.TimeRegisterValue.Timestamp.ToString("o") };

      return JsonSerializer.Serialize(payload);
    }

  }
}
