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

  }
}

