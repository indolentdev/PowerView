using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
    public class CostBreakdown
    {
        public CostBreakdown(string title, Unit currency, int vat, IList<CostBreakdownEntry> entries)
        {
            ArgCheck.ThrowIfNullOrEmpty(title);
            if (currency != Unit.Eur && currency != Unit.Dkk) throw new ArgumentOutOfRangeException(nameof(currency), $"Must be Eur or Dkk. Was:{currency}");
            if (vat < 0 || vat > 100) throw new ArgumentOutOfRangeException(nameof(vat), $"Must be betwen 0 and 100. Was:{vat}");
            ArgumentNullException.ThrowIfNull(entries);
            if (entries.Any(e => e == null)) throw new ArgumentNullException(nameof(entries), "Items must not be null");

            Title = title;
            Currency = currency;
            Vat = vat;
            Entries = entries.ToList();
        }

        public string Title { get; private set; }
        public Unit Currency { get; private set; }
        public int Vat { get; private set; }
        public IReadOnlyList<CostBreakdownEntry> Entries { get; private set; }

        public IDictionary<(DateTime FromDate, DateTime ToDate), IReadOnlyList<CostBreakdownEntry>> GetEntriesByPeriods()
        {
            var byFromDateToDate = Entries.OrderBy(x => x.FromDate).ThenBy(x => x.ToDate).ToList();

            var groups = GetPeriodGroups(byFromDateToDate);

            var result = new Dictionary<(DateTime FromDate, DateTime ToDate), IReadOnlyList<CostBreakdownEntry>>();
            foreach (var group in groups)
            {
                var entryList = new List<CostBreakdownEntry>();
                result.Add(group, entryList);
                foreach (var entry in byFromDateToDate)
                {
                    if (
                        (entry.FromDate <= group.FromDate && entry.ToDate > group.FromDate) ||
                        (entry.FromDate >= group.FromDate && entry.ToDate <= group.ToDate) ||
                        (entry.FromDate < group.ToDate && entry.ToDate >= group.ToDate)
                    )
                    {
                        entryList.Add(entry);
                    }
                }
            }

            return result;
        }

        private static List<(DateTime FromDate, DateTime ToDate)> GetPeriodGroups(List<CostBreakdownEntry> byFromDateToDate)
        {
            var byFromDates = byFromDateToDate.Select(x => (DateTime?)x.FromDate).Distinct().ToList();
            var byToDates = byFromDateToDate.Select(x => x.ToDate).OrderBy(x => x).Distinct().ToList();

            var groups = new List<(DateTime FromDate, DateTime ToDate)>();

            if (byFromDates.Count == 0) return groups;

            var fromDate = byFromDates.First().Value;
            while (fromDate < byToDates.Last())
            {
                var toDate1 = byFromDates.FirstOrDefault(x => x > fromDate);
                var toDate2 = byToDates.First(x => x > fromDate);

                var toDate = toDate2;
                var inc = true;
                if (toDate1 != null && toDate1.Value < toDate2)
                {
                    toDate = toDate1.Value;
                    inc = false;
                }
                groups.Add((fromDate, toDate));

                fromDate = toDate + (inc ? TimeSpan.FromDays(1) : TimeSpan.Zero);
            }

            return groups;
        }

        public IEnumerable<(NormalizedDurationRegisterValue Value, ICollection<CostBreakdownEntry> Entries)> Apply(ILocationContext locationContext, IEnumerable<NormalizedDurationRegisterValue> values)
        {
            ArgumentNullException.ThrowIfNull(locationContext);
            ArgumentNullException.ThrowIfNull(values);

            return ApplyInternal(locationContext, values).ToList();
        }

        private IEnumerable<(NormalizedDurationRegisterValue Value, ICollection<CostBreakdownEntry> Entries)> ApplyInternal(ILocationContext locationContext, IEnumerable<NormalizedDurationRegisterValue> values)
        {
            foreach (var value in values)
            {
                if (value.UnitValue.Unit != Currency)
                {
                    throw new DataMisalignedException($"Currency not compatible. Expected:{Currency}. Was:{value.UnitValue.Unit}");
                }

                var applyingEntries = Entries
                  .Where(x => x.AppliesToDates(value.NormalizedStart, value.NormalizedEnd)) // Match on entry UTC from and to dates
                  .Where(x => x.AppliesToTime(
                    TimeOnly.FromDateTime(locationContext.ConvertTimeFromUtc(value.NormalizedStart))) // Match on entry local time hours.
                  )
                  .ToList();
                var applyingEntriesAmount = applyingEntries.Sum(x => x.Amount);
                var amountExclVat = value.UnitValue.Value + applyingEntriesAmount;
                var amountInclVat = amountExclVat * ((Vat * 0.01) + 1);

                var newValue = new NormalizedDurationRegisterValue(value.Start, value.End, value.NormalizedStart, value.NormalizedEnd,
                  new UnitValue(amountInclVat, value.UnitValue.Unit), value.DeviceIds.Concat(new[] { Title }));

                yield return (newValue, applyingEntries);
            }
        }

    }
}

