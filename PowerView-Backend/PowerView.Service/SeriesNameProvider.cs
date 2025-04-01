using System;
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Model.Repository;


namespace PowerView.Service
{
    internal class SeriesNameProvider : ISeriesNameProvider
    {
        private readonly ILocationContext locationcontext;
        private readonly ISeriesNameRepository seriesNameRepository;
        private readonly ICostBreakdownGeneratorSeriesRepository costBreakdownGeneratorSeriesRepository;


        public SeriesNameProvider(ILocationContext locationContext, ISeriesNameRepository seriesNameRepository, ICostBreakdownGeneratorSeriesRepository costBreakdownGeneratorSeriesRepository)
        {
            this.locationcontext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
            this.seriesNameRepository = seriesNameRepository ?? throw new ArgumentNullException(nameof(seriesNameRepository));
            this.costBreakdownGeneratorSeriesRepository = costBreakdownGeneratorSeriesRepository ?? throw new ArgumentNullException(nameof(costBreakdownGeneratorSeriesRepository));
        }

        public IList<SeriesName> GetSeriesNames()
        {
            var seriesNames = seriesNameRepository.GetSeriesNames();
            var costBreakdownGeneratorSeries = costBreakdownGeneratorSeriesRepository.GetCostBreakdownGeneratorSeries();

            var intervalGroup = CreateIntervalGroup(seriesNames, costBreakdownGeneratorSeries);
            intervalGroup.Prepare();

            var actualSeriesNames = intervalGroup.NormalizedDurationLabelSeriesSet
              .SelectMany(ls => ls.Select(oc => new SeriesName(ls.Label, oc)))
              .ToList();
            return actualSeriesNames;
        }

        private IntervalGroup CreateIntervalGroup(IList<SeriesName> seriesNames, IReadOnlyList<CostBreakdownGeneratorSeries> costBreakdownGeneratorSeries)
        {
            // Conjure some data to make the series generation work.
            var seriesNamesUnits = new Dictionary<SeriesName, Unit>();
            foreach (var costBreakdownGenS in costBreakdownGeneratorSeries)
            {
                seriesNamesUnits.Add(costBreakdownGenS.GeneratorSeries.BaseSeries, costBreakdownGenS.CostBreakdown.Currency);
            }
            foreach (var seriesName in seriesNames)
            {
                seriesNamesUnits.TryAdd(seriesName, Unit.Joule);
            }

            var nowLocal = locationcontext.ConvertTimeFromUtc(DateTime.UtcNow);
            var midnight = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, nowLocal.Kind).ToUniversalTime();
            var dt1 = midnight;
            var dt2 = midnight.AddMinutes(60);

            var labelGroups = seriesNames.GroupBy(x => (string)x.Label, x => (ObisCode)(long)x.ObisCode);
            var labelSeries = new List<TimeRegisterValueLabelSeries>(8);
            foreach (var labelToObisCodes in labelGroups)
            {
                var label = labelToObisCodes.Key;
                var fakeObisToTimeRegisterValues = labelToObisCodes
                  .Distinct()
                  .ToDictionary(oc => oc, oc => (IEnumerable<TimeRegisterValue>)CreateTimeRegisterValues(dt1, dt2, seriesNamesUnits[new SeriesName(label, oc)]));
                var labelS = new TimeRegisterValueLabelSeries(label, fakeObisToTimeRegisterValues);
                labelSeries.Add(labelS);
            }
            var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(dt1, dt2, labelSeries);
            var intervalGroup = new IntervalGroup(locationcontext, midnight, "60-minutes", labelSeriesSet, costBreakdownGeneratorSeries);

            return intervalGroup;
        }

        private static TimeRegisterValue[] CreateTimeRegisterValues(DateTime dt1, DateTime dt2, Unit unit)
        {
            var fakeTimeRegisterValues = new TimeRegisterValue[]
            {
                new TimeRegisterValue("1", dt1, 1, 0, unit),
                new TimeRegisterValue("1", dt2, 2, 0, unit)
            };
            return fakeTimeRegisterValues;
        }

    }
}

