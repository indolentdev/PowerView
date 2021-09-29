using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  internal class ObisColorProvider : IObisColorProvider
  {
    private readonly IDictionary<ObisCode, string> colorMap = new Dictionary<ObisCode, string> 
    {
      { ObisCode.ElectrActualPowerP14, "#FFCC00" },
      { ObisCode.ElectrActualPowerP14Average, "#FFCC00" },
      { ObisCode.ElectrActiveEnergyA14Delta, "#FFCC00" },
      { ObisCode.ElectrActiveEnergyA14Period, "#FFF5CC" },
      { ObisCode.ElectrActiveEnergyA14NetDelta, "#FFCC00" },
      { ObisCode.ElectrActualPowerP23, "#FFFF00" },
      { ObisCode.ElectrActualPowerP23Average, "#FFFF00" },
      { ObisCode.ElectrActiveEnergyA23Delta, "#FFFF00" },
      { ObisCode.ElectrActiveEnergyA23Period, "#FFFFCC" },
      { ObisCode.ElectrActiveEnergyA23NetDelta, "#FFFF00" },
      { ObisCode.ColdWaterFlow1, "#330066" },
      { ObisCode.ColdWaterFlow1Average, "#330066" },
      { ObisCode.ColdWaterVolume1Delta, "#330066" }, 
      { ObisCode.ColdWaterVolume1Period, "#99CCFF" },
      { ObisCode.HotWaterFlow1, "#33CCCC" },
      { ObisCode.HotWaterFlow1Average, "#33CCCC" },
      { ObisCode.HotWaterVolume1Delta, "#33CCCC" },
      { ObisCode.HotWaterVolume1Period, "#D6F5F5" },
      { ObisCode.HeatEnergyPower1, "#CC3300" },
      { ObisCode.HeatEnergyPower1Average, "#CC3300" },
      { ObisCode.HeatEnergyEnergy1Delta, "#CC3300" },
      { ObisCode.HeatEnergyEnergy1Period, "#FF6699" },
      { ObisCode.HeatEnergyFlow1, "#33CCCC" },
      { ObisCode.HeatEnergyFlow1Average, "#33CCCC" },
      { ObisCode.HeatEnergyVolume1Delta, "#33CCCC" },
      { ObisCode.HeatEnergyVolume1Period, "#D6F5F5" },
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

