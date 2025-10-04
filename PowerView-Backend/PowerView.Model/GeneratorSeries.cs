using System;
namespace PowerView.Model
{
    public class GeneratorSeries : IEquatable<GeneratorSeries>
    {
        public GeneratorSeries(SeriesName series, SeriesName baseSeries, string costBreakdownTitle)
        {
            if (string.IsNullOrEmpty(costBreakdownTitle)) throw new ArgumentNullException(nameof(costBreakdownTitle));

            Series = series ?? throw new ArgumentNullException(nameof(series));
            BaseSeries = baseSeries ?? throw new ArgumentNullException(nameof(baseSeries));
            CostBreakdownTitle = costBreakdownTitle;
        }

        public SeriesName Series { get; private set; }
        public SeriesName BaseSeries { get; private set; }
        public string CostBreakdownTitle { get; private set; }

        public bool SupportsInterval(string interval)
        {
            if (BaseSeries.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatQ && Series.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVatQ &&
              interval == "15-minutes")
            {
                return true;
            }
            if (BaseSeries.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatH && Series.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVatH &&
              interval == "60-minutes")
            {
                return true;
            }

            return false;
        }

        public bool SupportsDurations(IEnumerable<NormalizedDurationRegisterValue> values)
        {
            if (BaseSeries.ObisCode != ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatQ && BaseSeries.ObisCode != ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatH) return false;

            var minutes = new List<int>();
            minutes.Add(0);

            if (BaseSeries.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatQ)
            {
                minutes.Add(15);
                minutes.Add(30);
                minutes.Add(45);
            }

            foreach (var value in values)
                {
                    var timestamp = value.Start;
                    if (!minutes.Contains(timestamp.Minute) || timestamp.Second != 0) return false;
                }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GeneratorSeries)) return false;
            return Equals((GeneratorSeries)obj);
        }

        public bool Equals(GeneratorSeries other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
#pragma warning disable CA1309 // Use ordinal string comparison
            return Series.Equals(other.Series) && BaseSeries.Equals(other.BaseSeries) && CostBreakdownTitle.Equals(other.CostBreakdownTitle, StringComparison.InvariantCulture);
#pragma warning restore CA1309 // Use ordinal string comparison
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Series.GetHashCode();
                hashCode = (hashCode * 397) ^ BaseSeries.GetHashCode();
                hashCode = (hashCode * 397) ^ CostBreakdownTitle.GetHashCode();
                return hashCode;
            }
        }

    }

}
