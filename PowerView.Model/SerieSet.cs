using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class SerieSet
  {
    public SerieSet(string title, IEnumerable<DateTime> categories, ICollection<Serie> series)
    {
      if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
      if (categories == null) throw new ArgumentNullException("categories");
      if (series == null) throw new ArgumentNullException("series");

      Title = title;
      Categories = categories.ToArray();
      Series = series;
    }

    public string Title { get; private set; }
    public DateTime[] Categories { get; private set; }
    public ICollection<Serie> Series { get; private set; }
  }
}

