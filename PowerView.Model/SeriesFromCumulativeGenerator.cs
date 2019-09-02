using System.Collections.Generic;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model
{
  public class SeriesFromCumulativeGenerator
  {
    public IDictionary<ObisCode, IList<TimeRegisterValue>> Generate(IDictionary<ObisCode, IList<TimeRegisterValue>> cumulatives)
    {
      var result = new Dictionary<ObisCode, IList<TimeRegisterValue>>(3);

      foreach (var cumulative in cumulatives)
      {
        if (!cumulative.Key.IsCumulative) continue;

        var periodGenerator = new { ObisCode = cumulative.Key.ToPeriod(), Generator = new PeriodSeriesGenerator() };
        var deltaGenerator = new { ObisCode = cumulative.Key.ToDelta(), Generator = new DeltaSeriesGenerator() };
        var actualAverageGenerator = new { ObisCode = GetAverageActualObisCode(cumulative.Key), Generator = new AverageActualSeriesGenerator() };

        foreach (var value in cumulative.Value)
        {
          periodGenerator.Generator.CalculateNext(value);
          deltaGenerator.Generator.CalculateNext(value);
          if (actualAverageGenerator.ObisCode != null) actualAverageGenerator.Generator.CalculateNext(value);
        }

        result.Add(periodGenerator.ObisCode, periodGenerator.Generator.GetGenerated());
        result.Add(deltaGenerator.ObisCode, deltaGenerator.Generator.GetGenerated());
        if (actualAverageGenerator.ObisCode != null)
        {
          result.Add(actualAverageGenerator.ObisCode.Value, actualAverageGenerator.Generator.GetGenerated());
        }
      }

      return result;
    }

    private static ObisCode? GetAverageActualObisCode(ObisCode obisCode)
    {
      if (obisCode == ObisCode.ElectrActiveEnergyA14)
      {
        return ObisCode.ElectrActualPowerP14Average;
      }
      else if (obisCode == ObisCode.ElectrActiveEnergyA23)
      {
        return ObisCode.ElectrActualPowerP23Average;
      }
      else if (obisCode == ObisCode.ColdWaterVolume1)
      {
        return ObisCode.ColdWaterFlow1Average;
      }
      else if (obisCode == ObisCode.HeatEnergyEnergy1)
      {
        return ObisCode.HeatEnergyPower1Average;
      }
      else if (obisCode == ObisCode.HeatEnergyVolume1)
      {
        return ObisCode.HeatEnergyFlow1Average;
      }

      return null;
    }
  }
}
