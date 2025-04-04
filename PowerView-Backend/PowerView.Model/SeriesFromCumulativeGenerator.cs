﻿using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model.SeriesGenerators;

namespace PowerView.Model
{
    public class SeriesFromCumulativeGenerator
    {
        internal static IDictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>> Generate(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> cumulativeLabelSeries)
        {
            var result = new Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>>();

            GenerateSingleInputSeries(cumulativeLabelSeries, result);

            GenerateMultiInputSeries(result);

            return result;
        }

        private static void GenerateSingleInputSeries(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> cumulatives, Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>> result)
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

                result.Add(periodGenerator.ObisCode, periodGenerator.Generator.GetGeneratedDurations());
                result.Add(deltaGenerator.ObisCode, deltaGenerator.Generator.GetGeneratedDurations());
                if (actualAverageGenerator.ObisCode != null)
                {
                    result.Add(actualAverageGenerator.ObisCode.Value, actualAverageGenerator.Generator.GetGeneratedDurations());
                }
            }
        }

        private static void GenerateMultiInputSeries(Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>> result)
        {
            var activeEnergyA14NetDeltaGenerator = new
            {
                ObisCode = ObisCode.ElectrActiveEnergyA14NetDelta,
                Generator = new DiffByTimeSeriesGenerator(ObisCode.ElectrActiveEnergyA14Delta, ObisCode.ElectrActiveEnergyA23Delta)
            };
            var activeEnergyA23NetDeltaGenerator = new
            {
                ObisCode = ObisCode.ElectrActiveEnergyA23NetDelta,
                Generator = new DiffByTimeSeriesGenerator(ObisCode.ElectrActiveEnergyA23Delta, ObisCode.ElectrActiveEnergyA14Delta)
            };

            var generators = new[] { activeEnergyA14NetDeltaGenerator, activeEnergyA23NetDeltaGenerator };

            var satisfiedGenerators = generators.Where(x => x.Generator.IsSatisfiedBy(result)).ToList();

            if (satisfiedGenerators.Count == 0)
            {
                return;
            }

            // "flip" the dictionary so it is keyed first by normalized end time, and then by obis code.
            var generatedByNormalizedTimestamp = result
              .SelectMany(x => x.Value, (x, y) => new { ObisCode = x.Key, NormalizedDurationRegisterValue = y })
              .GroupBy(x => x.NormalizedDurationRegisterValue.NormalizedEnd)
              .OrderBy(x => x.Key)
              .ToDictionary(x => x.Key, x => x.ToDictionary(xx => xx.ObisCode, xx => xx.NormalizedDurationRegisterValue));

            foreach (var item in generatedByNormalizedTimestamp)
            {
                foreach (var generator in satisfiedGenerators)
                {
                    generator.Generator.CalculateNext(item.Value);
                }
            }

            foreach (var generator in satisfiedGenerators)
            {
                result.Add(generator.ObisCode, generator.Generator.GetGenerated());
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

    }
}
