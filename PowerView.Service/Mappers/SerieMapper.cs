using System.Reflection;
using PowerView.Model;
using log4net;

namespace PowerView.Service.Mappers
{
  internal class SerieMapper : ISerieMapper
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public string MapToSerieType(ObisCode obisCode)
    {
      if (obisCode == ObisCode.ActiveEnergyA14Interim || obisCode == ObisCode.ActiveEnergyA23Interim ||
         obisCode == ObisCode.ColdWaterVolume1Interim || obisCode == ObisCode.HeatEnergyEnergy1Interim ||
         obisCode == ObisCode.HeatEnergyVolume1Interim ||
         obisCode == ObisCode.ConsumedElectricityInterim || obisCode == ObisCode.ConsumedElectricityWithHeatInterim)
      {
        return "areaspline";
      }

      return "spline";
    }

    public string MapToSerieYAxis(ObisCode obisCode)
    {
      if (obisCode == ObisCode.ActiveEnergyA14Interim || obisCode == ObisCode.ActiveEnergyA23Interim ||
         obisCode == ObisCode.HeatEnergyEnergy1Interim || 
         obisCode == ObisCode.ConsumedElectricityInterim || obisCode == ObisCode.ConsumedElectricityWithHeatInterim )
      {
        return "energyInterim";
      }

      if (obisCode == ObisCode.ActiveEnergyA14Delta || obisCode == ObisCode.ActiveEnergyA23Delta ||
         obisCode == ObisCode.HeatEnergyEnergy1Delta || 
         obisCode == ObisCode.ConsumedElectricityDelta || obisCode == ObisCode.ConsumedElectricityWithHeatDelta)
      {
        return "energyDelta";
      }

      if (obisCode == ObisCode.ActualPowerP14 || obisCode == ObisCode.ActualPowerP23 ||
        obisCode == ObisCode.HeatEnergyPower1 || obisCode == ObisCode.ActualPowerP14L1 ||
         obisCode == ObisCode.ActualPowerP14L2 || obisCode == ObisCode.ActualPowerP14L3 ||
         obisCode == ObisCode.ActualPowerP23L1 || obisCode == ObisCode.ActualPowerP23L2 ||
         obisCode == ObisCode.ActualPowerP23L3)
      {
        return "power";
      }

      if (obisCode == ObisCode.ColdWaterVolume1Interim)
      {
        return "volumeInterim";
      }

      if (obisCode == ObisCode.HeatEnergyVolume1Interim)
      {
        return "volumeInterimHiddenYAxis";
      }

      if (obisCode == ObisCode.ColdWaterVolume1Delta)
      {
        return "volumeDelta";
      }

      if (obisCode == ObisCode.HeatEnergyVolume1Delta)
      {
        return "volumeDeltaHiddenYAxis";
      }

      if (obisCode == ObisCode.ColdWaterFlow1)
      {
        return "flow";
      }

      if (obisCode == ObisCode.HeatEnergyFlow1)
      {
        return "flowHiddenYAxis";
      }

      if (obisCode == ObisCode.RoomTemperature)
      {
        return "temp";
      }

      if (obisCode == ObisCode.HeatEnergyFlowTemperature || obisCode == ObisCode.HeatEnergyReturnTemperature)
      {
        return "tempHiddenYAxis";
      }

      if (obisCode == ObisCode.RoomRelativeHumidity)
      {
        return "rh";
      }

      if (obisCode.IsDisconnectControl)
      {
        return "dcOutputStatusHiddenYAxis";
      }

      log.InfoFormat("Unable to map obis code {0} to graph y-axis", obisCode);

      return string.Empty;
    }
  }
}

