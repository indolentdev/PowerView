using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerView.Model
{
    public class IntervalGroup
    {
        private readonly ILocationContext locationContext;
        private readonly Func<DateTime, DateTime> timeDivider;
        private readonly Func<DateTime, DateTime> getNext;

        public IntervalGroup(ILocationContext locationContext, DateTime start, string interval, TimeRegisterValueLabelSeriesSet labelSeriesSet, IReadOnlyList<CostBreakdownGeneratorSeries> costBreakdownGeneratorSeries)
        {
            this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));

            var dateTimeHelper = new DateTimeHelper(locationContext, start);
            timeDivider = dateTimeHelper.GetDivider(interval);
            getNext = dateTimeHelper.GetNext(interval);

            Interval = interval;
            LabelSeriesSet = labelSeriesSet ?? throw new ArgumentNullException(nameof(labelSeriesSet));
            CostBreakdownGeneratorSeries = costBreakdownGeneratorSeries ?? throw new ArgumentNullException(nameof(costBreakdownGeneratorSeries));
        }

        public string Interval { get; }
        public TimeRegisterValueLabelSeriesSet LabelSeriesSet { get; }
        public IReadOnlyList<CostBreakdownGeneratorSeries> CostBreakdownGeneratorSeries { get; }

        public IReadOnlyList<DateTime> Categories { get; private set; }
        public LabelSeriesSet<NormalizedTimeRegisterValue> NormalizedLabelSeriesSet { get; private set; }
        public LabelSeriesSet<NormalizedDurationRegisterValue> NormalizedDurationLabelSeriesSet { get; private set; }

        public void Prepare()
        {
            Categories = new ReadOnlyCollection<DateTime>(GetCategories());

            NormalizedLabelSeriesSet = LabelSeriesSet.Normalize(timeDivider);

            NormalizedDurationLabelSeriesSet = new LabelSeriesSet<NormalizedDurationRegisterValue>(
              NormalizedLabelSeriesSet.Start,
              NormalizedLabelSeriesSet.End,
              NormalizedLabelSeriesSet.Count() * 2,
              TransformToNormalizedDurations(NormalizedLabelSeriesSet));
        }

        private List<DateTime> GetCategories()
        {
            var categories = new List<DateTime>();
            var categoryTimestamp = LabelSeriesSet.Start;
            while (categoryTimestamp < LabelSeriesSet.End)
            {
                categories.Add(categoryTimestamp);
                categoryTimestamp = getNext(categoryTimestamp);
            }

            return categories;
        }

        private IEnumerable<LabelSeries<NormalizedDurationRegisterValue>> TransformToNormalizedDurations(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
        {
            var costBreakdownGeneratorSeriesByBaseSeriesName = CostBreakdownGeneratorSeries.ToLookup(x => x.GeneratorSeries.BaseSeries);

            var result = new Dictionary<string, IDictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>>>();
            foreach (var labelSeries in labelSeriesSet)
            {
                var label = labelSeries.Label;
                if (!result.TryGetValue(label, out var obisDurationValues))
                {
                    obisDurationValues = new Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>>();
                    result.Add(label, obisDurationValues);
                }

                // Transform non-cumulative series to normalized non-cumulative series.
                var nonCumulativeLabelSeries = labelSeries.GetNonCumulativeSeries();
                foreach (var item in nonCumulativeLabelSeries)
                {
                    var durationValues = item.Value.Select(x => new NormalizedDurationRegisterValue(x.TimeRegisterValue.Timestamp, x.TimeRegisterValue.Timestamp,
                      x.NormalizedTimestamp, x.NormalizedTimestamp, x.TimeRegisterValue.UnitValue, x.TimeRegisterValue.DeviceId)).ToList();
                    obisDurationValues.Add(item.Key, durationValues);
                }

                // Generate additional series from normalized non-cumulative series.
                foreach (var obisCode in obisDurationValues.Keys.ToList())
                {
                    var seriesName = new SeriesName(label, obisCode);
                    foreach (var costBreakdownGeneratorSeries in costBreakdownGeneratorSeriesByBaseSeriesName[seriesName])
                    {
                        var baseSeries = obisDurationValues[obisCode];
                        var generatedSeriesName = costBreakdownGeneratorSeries.GeneratorSeries.Series;
                        try
                        {
                            if (!costBreakdownGeneratorSeries.GeneratorSeries.SupportsInterval(Interval)) continue;
                            if (!costBreakdownGeneratorSeries.GeneratorSeries.SupportsDurations(baseSeries)) continue;

                            var generatedResult = costBreakdownGeneratorSeries.CostBreakdown.Apply(locationContext, baseSeries).ToList();

                            if (generatedResult.Where(x => x.Entries.Count == 0).Any()) continue; // Only include the series if all items had generated values

                            var generatedValues = generatedResult.Select(x => x.Value).ToList();
                            if (generatedSeriesName.Label == labelSeries.Label)
                            {
                                obisDurationValues.Add(generatedSeriesName.ObisCode, generatedValues);
                            }
                            else
                            {
                                if (!result.TryGetValue(generatedSeriesName.Label, out var labelValues))
                                {
                                    labelValues = new Dictionary<ObisCode, ICollection<NormalizedDurationRegisterValue>>();
                                    result.Add(generatedSeriesName.Label, labelValues);
                                }
                                labelValues.Add(generatedSeriesName.ObisCode, generatedValues);
                            }
                        }
                        catch (DataMisalignedException)
                        {  // Something went wrong with series generation. Skip that generated series.
                        }
                    }
                }

                // Transform cumulative series by generating "spin-off" normalized non-cumulative series.
                var cumulativeLabelSeries = labelSeries.GetCumulativeSeries();
                if (cumulativeLabelSeries.Count > 0)
                {
                    var durationValues = SeriesFromCumulativeGenerator.Generate(cumulativeLabelSeries);
                    foreach (var item in durationValues)
                    {
                        obisDurationValues.Add(item.Key, item.Value);
                    }
                }
            }

            foreach (var res in result)
            {
                var d = new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>>();
                foreach (var item in res.Value)
                {
                    d.Add(item.Key, item.Value);
                }
                yield return new LabelSeries<NormalizedDurationRegisterValue>(res.Key, d);
            }
        }
    }
}
