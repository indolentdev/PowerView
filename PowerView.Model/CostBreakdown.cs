using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class CostBreakdown
  {
    public CostBreakdown(string title, Unit currency, int vat, IList<CostBreakdownEntry> entries)
    {
      if (string.IsNullOrEmpty(title)) throw new ArgumentOutOfRangeException(nameof(title));
      if (currency != Unit.Eur && currency != Unit.Dkk) throw new ArgumentOutOfRangeException(nameof(currency), $"Must be Eur or Dkk. Was:{currency}");
      if (vat < 0 || vat > 100) throw new ArgumentOutOfRangeException(nameof(vat), $"Must be betwen 0 and 100. Was:{vat}");
      if (entries == null) throw new ArgumentNullException(nameof(entries));
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
          if (entry.FromDate >= group.FromDate && entry.ToDate <= group.ToDate)
          {
            entryList.Add(entry);
          }
        }
      }

      return result;
    }

    private static List<(DateTime FromDate, DateTime ToDate)> GetPeriodGroups(List<CostBreakdownEntry> byFromDateToDate)
    {
      var groups = new List<(DateTime FromDate, DateTime ToDate)>();

      foreach (var e in byFromDateToDate)
      {
        if (groups.Count == 0)
        {
          groups.Add((e.FromDate, e.ToDate));
          continue;
        }

        var previous = groups.Last();
        if (e.FromDate == previous.FromDate && e.ToDate == previous.ToDate) continue;

        if (e.FromDate > previous.FromDate)
        {
          groups.Add((e.FromDate, e.ToDate));
          continue;
        }

        if (e.ToDate > previous.ToDate)
        {
          groups.Add((e.FromDate, e.ToDate));
        }
      }

      return groups;
    }

  }
}

