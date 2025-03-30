using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
    public class Series
    {
        public Series(SeriesName serieName, Unit unit, IEnumerable<DeviationValue?> values)
        {
            ArgumentNullException.ThrowIfNull(serieName);
            ArgumentNullException.ThrowIfNull(values);

            SeriesName = serieName;
            Unit = unit;
            Values = values.ToList();
        }

        public SeriesName SeriesName { get; }
        public Unit Unit { get; }
        public IReadOnlyList<DeviationValue?> Values { get; }
    }
}
