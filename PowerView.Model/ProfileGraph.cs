using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class ProfileGraph
  {
    public ProfileGraph(string period, string page, string title, string interval, long rank, IList<SeriesName> serieNames)
    {
      if (string.IsNullOrEmpty(period)) throw new ArgumentNullException("period");
      if (!(new[] { "day", "month", "year" }.Contains(period))) throw new ArgumentOutOfRangeException("period", period, "Invalid period");
      if (page == null) throw new ArgumentNullException("page");
      if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
      if (string.IsNullOrEmpty(interval)) throw new ArgumentNullException("interval");
      DateTimeResolutionDivider.GetResolutionDivider(interval);
      if (serieNames == null) throw new ArgumentNullException("serieNames");
      if (serieNames.Any(gv => gv == null)) throw new ArgumentNullException("serieNames", "Items must not be null");
      if (serieNames.Count == 0) throw new ArgumentException("Must have at least one serie", "serieNames");
      if (serieNames.Count != serieNames.Distinct().Count()) throw new ArgumentException("Must not have duplicate serie entries", "serieNames");

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

