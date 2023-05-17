using System.Reflection;
using PowerView.Model;
using Microsoft.Extensions.Logging;

namespace PowerView.Service.Mappers
{
    internal class SerieMapper : ISerieMapper
    {
        private readonly ILogger logger;

        public SerieMapper(ILogger<SerieMapper> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string MapToSerieType(ObisCode obisCode)
        {
            if (obisCode == ObisCode.ElectrActiveEnergyA14Period || obisCode == ObisCode.ElectrActiveEnergyA23Period ||

               obisCode == ObisCode.ColdWaterVolume1Period || obisCode == ObisCode.HotWaterVolume1Period ||
               obisCode == ObisCode.HeatEnergyEnergy1Period || obisCode == ObisCode.HeatEnergyVolume1Period)
            {
                return "areaspline";
            }

            return "spline";
        }

        public string MapToSerieYAxis(ObisCode obisCode)
        {
            if (obisCode == ObisCode.ElectrActiveEnergyA14Period || obisCode == ObisCode.ElectrActiveEnergyA23Period ||
               obisCode == ObisCode.HeatEnergyEnergy1Period)
            {
                return "energyPeriod";
            }

            if (obisCode == ObisCode.ElectrActiveEnergyA14Delta || obisCode == ObisCode.ElectrActiveEnergyA23Delta ||
               obisCode == ObisCode.ElectrActiveEnergyA14NetDelta || obisCode == ObisCode.ElectrActiveEnergyA23NetDelta ||
              obisCode == ObisCode.HeatEnergyEnergy1Delta)
            {
                return "energyDelta";
            }

            if (obisCode == ObisCode.ElectrActualPowerP14 || obisCode == ObisCode.ElectrActualPowerP14Average ||
                obisCode == ObisCode.ElectrActualPowerP23 || obisCode == ObisCode.ElectrActualPowerP23Average ||
                obisCode == ObisCode.HeatEnergyPower1 || obisCode == ObisCode.HeatEnergyPower1Average ||
                obisCode == ObisCode.ElectrActualPowerP14L1 ||
                obisCode == ObisCode.ElectrActualPowerP14L2 || obisCode == ObisCode.ElectrActualPowerP14L3 ||
                obisCode == ObisCode.ElectrActualPowerP23L1 || obisCode == ObisCode.ElectrActualPowerP23L2 ||
                obisCode == ObisCode.ElectrActualPowerP23L3)
            {
                return "power";
            }

            if (obisCode == ObisCode.ColdWaterVolume1Period || obisCode == ObisCode.HotWaterVolume1Period)
            {
                return "volumePeriod";
            }

            if (obisCode == ObisCode.HeatEnergyVolume1Period)
            {
                return "volumePeriodHiddenYAxis";
            }

            if (obisCode == ObisCode.ColdWaterVolume1Delta || obisCode == ObisCode.HotWaterVolume1Delta)
            {
                return "volumeDelta";
            }

            if (obisCode == ObisCode.HeatEnergyVolume1Delta)
            {
                return "volumeDeltaHiddenYAxis";
            }

            if (obisCode == ObisCode.ColdWaterFlow1 || obisCode == ObisCode.ColdWaterFlow1Average ||
              obisCode == ObisCode.HotWaterFlow1 || obisCode == ObisCode.HotWaterFlow1Average
            )
            {
                return "flow";
            }

            if (obisCode == ObisCode.HeatEnergyFlow1 || obisCode == ObisCode.HeatEnergyFlow1Average)
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

            if (obisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense)
            {
                return "currencyAmount";
            }

            logger.LogInformation("Unable to map obis code {0} to graph y-axis", obisCode);

            return string.Empty;
        }
    }
}

