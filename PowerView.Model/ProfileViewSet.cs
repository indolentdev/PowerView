using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class ProfileViewSet
  {
    public ProfileViewSet(ICollection<SerieSet> serieSets, ICollection<NamedValue> interimTotals)
    {
      if (serieSets == null) throw new ArgumentNullException("serieSets");
      if (serieSets.Where(x => x == null).Any()) throw new ArgumentOutOfRangeException("serieSets");
      if (interimTotals == null) throw new ArgumentNullException("interimTotals");
      if (interimTotals.Where(x => x == null).Any()) throw new ArgumentOutOfRangeException("interimTotals");

      SerieSets = serieSets;
      PeriodTotals = interimTotals;
    }

    public ICollection<SerieSet> SerieSets { get; private set; }
    public ICollection<NamedValue> PeriodTotals { get; private set; }
  }
}

