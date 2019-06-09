using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  internal class ObisColorProvider : IObisColorProvider
  {
    private readonly IDictionary<ObisCode, string> colorMap = new Dictionary<ObisCode, string> 
    {
      { ObisCode.ActualPowerP14, "#FFCC00" },
      { ObisCode.ActiveEnergyA14Delta, "#FFCC00" },
      { ObisCode.ActiveEnergyA14Interim, "#FFF5CC" },
      { ObisCode.ActualPowerP23, "#FFFF00" },
      { ObisCode.ActiveEnergyA23Delta, "#FFFF00" },
      { ObisCode.ActiveEnergyA23Interim, "#FFFFCC" },
      { ObisCode.ColdWaterFlow1, "#330066" },
      { ObisCode.ColdWaterVolume1Delta, "#330066" }, 
      { ObisCode.ColdWaterVolume1Interim, "#99CCFF" }, 
      { ObisCode.HeatEnergyPower1, "#CC3300" },
      { ObisCode.HeatEnergyEnergy1Delta, "#CC3300" },
      { ObisCode.HeatEnergyEnergy1Interim, "#FF6699" },
      { ObisCode.HeatEnergyFlow1, "#33CCCC" },
      { ObisCode.HeatEnergyVolume1Delta, "#33CCCC" },
      { ObisCode.HeatEnergyVolume1Interim, "#D6F5F5" },
      { ObisCode.HeatEnergyFlowTemperature, "#E60000" },
      { ObisCode.HeatEnergyReturnTemperature, "#3333CC" },
      { ObisCode.RoomTemperature, "#FF99FF" },
      { ObisCode.RoomRelativeHumidity, "#E600E6" },
    };

    public string GetColor(ObisCode obisCode)
    {
      if (colorMap.ContainsKey(obisCode))
      {
        return colorMap[obisCode];
      }

      return "#000000"; 
    }

  }
}

