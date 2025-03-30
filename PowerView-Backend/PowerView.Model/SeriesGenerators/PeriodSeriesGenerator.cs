using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.SeriesGenerators
{
    public class PeriodSeriesGenerator : ISingleInputSeriesGenerator
    {
        private readonly List<NormalizedTimeRegisterValue> snReferenceValues;
        private readonly Dictionary<string, NormalizedDurationRegisterValue> snTransitionValues;
        private readonly List<NormalizedDurationRegisterValue> generatedValues;

        public PeriodSeriesGenerator()
        {
            snReferenceValues = new List<NormalizedTimeRegisterValue>(2);
            snTransitionValues = new Dictionary<string, NormalizedDurationRegisterValue>(2);
            generatedValues = new List<NormalizedDurationRegisterValue>(300);
        }

        public void CalculateNext(NormalizedTimeRegisterValue timeRegisterValue)
        {
            if (snReferenceValues.Count == 0)
            {
                snReferenceValues.Add(timeRegisterValue);
            }

            var reference = snReferenceValues[snReferenceValues.Count - 1];
            if (!reference.DeviceIdEquals(timeRegisterValue))
            {
                snReferenceValues.Add(timeRegisterValue);
                reference = timeRegisterValue;
            }

            var value = timeRegisterValue.SubtractAccommodateWrap(reference);
            snTransitionValues[GetTransitionKey(reference)] = value;

            var generatedValue = Sum(snTransitionValues.Values);
            generatedValues.Add(generatedValue);
        }

        public IList<NormalizedDurationRegisterValue> GetGeneratedDurations()
        {
            return generatedValues.AsReadOnly();
        }

        private static string GetTransitionKey(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
        {
            return $"SN:{normalizedTimeRegisterValue.TimeRegisterValue.DeviceId}-RefTime:{normalizedTimeRegisterValue.TimeRegisterValue.Timestamp.ToString("o")}";
        }

        private static NormalizedDurationRegisterValue Sum(ICollection<NormalizedDurationRegisterValue> normalizedTimeRegisterValues)
        {
            NormalizedDurationRegisterValue addend = null;
            foreach (var normalizedTimeRegisterValue in normalizedTimeRegisterValues.OrderBy(x => x.OrderProperty))
            {
                if (addend == null)
                {
                    addend = normalizedTimeRegisterValue;
                    continue;
                }

                if (normalizedTimeRegisterValue.UnitValue.Unit != addend.UnitValue.Unit)
                {
                    throw new DataMisalignedException("A calculation of a value sum was not possible. Units of values differ. Units:" +
                      normalizedTimeRegisterValue.UnitValue.Unit + ", " + addend.UnitValue.Unit);
                }

                addend = new NormalizedDurationRegisterValue(
                  addend.Start, normalizedTimeRegisterValue.End, addend.NormalizedStart, normalizedTimeRegisterValue.NormalizedEnd,
                  new UnitValue(addend.UnitValue.Value + normalizedTimeRegisterValue.UnitValue.Value, normalizedTimeRegisterValue.UnitValue.Unit),
                  addend.DeviceIds.Concat(normalizedTimeRegisterValue.DeviceIds));
            }
            return addend;
        }

    }
}
