using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class SeriesSet
  {
    public SeriesSet(string title, IEnumerable<DateTime> categories, ICollection<Series> series)
    {
      ArgCheck.ThrowIfNullOrEmpty(title);
      ArgumentNullException.ThrowIfNull(categories);
      ArgumentNullException.ThrowIfNull(series);

      Title = title;
      Categories = categories.ToArray();
      Series = series;
    }

    public string Title { get; private set; }
    public DateTime[] Categories { get; private set; }
    public ICollection<Series> Series { get; private set; }
  }
}

