using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
    public class ProfileViewSet
    {
        public ProfileViewSet(ICollection<SeriesSet> serieSets, ICollection<NamedValue> interimTotals)
        {
            ArgumentNullException.ThrowIfNull(serieSets);
            if (serieSets.Where(x => x == null).Any()) throw new ArgumentOutOfRangeException(nameof(serieSets));
            ArgumentNullException.ThrowIfNull(interimTotals);
            if (interimTotals.Where(x => x == null).Any()) throw new ArgumentOutOfRangeException(nameof(interimTotals));

            SerieSets = serieSets;
            PeriodTotals = interimTotals;
        }

        public ICollection<SeriesSet> SerieSets { get; private set; }
        public ICollection<NamedValue> PeriodTotals { get; private set; }
    }
}

