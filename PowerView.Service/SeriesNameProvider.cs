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
            var labelsAndObisCodes = seriesNameRepository.GetSeriesNames();
            var costBreakdownGeneratorSeries = costBreakdownGeneratorSeriesRepository.GetCostBreakdownGeneratorSeries();

            var nowLocal = locationcontext.ConvertTimeFromUtc(DateTime.UtcNow);
            var midnight = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, nowLocal.Kind).ToUniversalTime();
            var dt1 = midnight;
            var dt2 = midnight.AddMinutes(60);
            var fakeUnit = Unit.Joule;
            IEnumerable<TimeRegisterValue> fakeTimeRegisterValues = new List<TimeRegisterValue> { new TimeRegisterValue("1", dt1, 1, 0, fakeUnit), new TimeRegisterValue("1", dt2, 2, 0, fakeUnit) };

            var labelGroups = labelsAndObisCodes.GroupBy(x => (string)x.Label, x => (ObisCode)(long)x.ObisCode);
            var labelSeries = new List<TimeRegisterValueLabelSeries>(8);
            foreach (var labelToObisCodes in labelGroups)
            {
                var fakeObisToTimeRegisterValues = labelToObisCodes.Distinct().ToDictionary(oc => oc, x => fakeTimeRegisterValues);
                var labelS = new TimeRegisterValueLabelSeries(labelToObisCodes.Key, fakeObisToTimeRegisterValues);
                labelSeries.Add(labelS);
            }
            var labelSeriesSet = new TimeRegisterValueLabelSeriesSet(dt1, dt2, labelSeries);
            var intervalGroup = new IntervalGroup(locationcontext.TimeZoneInfo, midnight, "60-minutes", labelSeriesSet, costBreakdownGeneratorSeries);
            intervalGroup.Prepare();

            var seriesNames = intervalGroup.NormalizedDurationLabelSeriesSet
              .SelectMany(ls => ls.Select(oc => new SeriesName(ls.Label, oc)))
              .ToList();
            return seriesNames;
        }

    }
}

