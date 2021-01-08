using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model
{
  public class SeriesFromCumulativeGenerator
  {
    public IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> Generate(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> cumulatives)
    {
      var result = new Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>>();

      GenerateSingleInputSeries(cumulatives, result);

      GenerateMultiInputSeries(result);

      return result;
    }

    private static void GenerateSingleInputSeries(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> cumulatives, Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> result)
    {
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
      else if (obisCode == ObisCode.HotWaterVolume1)
      {
        return ObisCode.HotWaterFlow1Average;
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

    private void GenerateMultiInputSeries(Dictionary<ObisCode, IList<NormalizedTimeRegisterValue>> result)
    {
      var activeEnergyNetDeltaGenerator = new { ObisCode = ObisCode.ElectrActiveEnergyNetDelta, 
        Generator = new DiffByTimeSeriesGenerator(ObisCode.ElectrActiveEnergyA14Delta, ObisCode.ElectrActiveEnergyA23Delta) };
      var activeEnergyNetPeriodGenerator = new { ObisCode = ObisCode.ElectrActiveEnergyNetPeriod,
        Generator = new DiffByTimeSeriesGenerator(ObisCode.ElectrActiveEnergyA14Period, ObisCode.ElectrActiveEnergyA23Period) };

      var generators = new[] { activeEnergyNetDeltaGenerator, activeEnergyNetPeriodGenerator };

      var satisfiedGenerators = generators.Where(x => x.Generator.IsSatisfiedBy(result)).ToList();

      if (!satisfiedGenerators.Any())
      {
        return;
      }

      // "flip" the dictionary so it is keyed first by normalized time, and then by obis code.
      var generatedByNormalizedTimestamp = result
        .SelectMany(x => x.Value, (x, y) => new { ObisCode = x.Key, NormalizedTimeRegisterValue = y })
        .GroupBy(x => x.NormalizedTimeRegisterValue.NormalizedTimestamp)
        .ToDictionary(x => x.Key, x => x.ToDictionary(xx => xx.ObisCode, xx => xx.NormalizedTimeRegisterValue.TimeRegisterValue));

      foreach (var item in generatedByNormalizedTimestamp)
      {
        foreach (var generator in satisfiedGenerators)
        {
          generator.Generator.CalculateNext(item.Key, item.Value);
        }
      }

      foreach (var generator in satisfiedGenerators)
      {
        result.Add(generator.ObisCode, generator.Generator.GetGenerated());
      }
    }

  }
}
