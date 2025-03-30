using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
    public class ProfileGraph
    {
        private static readonly string[] periodNames = new[] { "day", "month", "year", "decade" };

        public ProfileGraph(string period, string page, string title, string interval, long rank, IList<SeriesName> serieNames)
        {
            ArgCheck.ThrowIfNullOrEmpty(period);
            if (!periodNames.Contains(period)) throw new ArgumentOutOfRangeException(nameof(period), period, "Invalid period");
            ArgumentNullException.ThrowIfNull(page);
            ArgCheck.ThrowIfNullOrEmpty(title);
            ArgCheck.ThrowIfNullOrEmpty(interval);
            ArgumentNullException.ThrowIfNull(serieNames);
            if (serieNames.Any(gv => gv == null)) throw new ArgumentNullException(nameof(serieNames), "Items must not be null");
            if (serieNames.Count == 0) throw new ArgumentException("Must have at least one serie", nameof(serieNames));
            if (serieNames.Count != serieNames.Distinct().Count()) throw new ArgumentException("Must not have duplicate serie entries", nameof(serieNames));

            Period = period;
            Page = page;
            Title = title;
            Interval = interval;
            Rank = rank;
            SerieNames = serieNames;
        }

        public string Period { get; private set; }
        public string Page { get; private set; }
        public string Title { get; private set; }
        public string Interval { get; private set; }
        public long Rank { get; private set; }
        public IList<SeriesName> SerieNames { get; private set; }
    }
}

