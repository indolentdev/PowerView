using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface ISeriesColorRepository
  {
    string GetColorCached(string label, ObisCode obisCode);

    ICollection<SeriesColor> GetSeriesColors();

    void SetSeriesColors(IEnumerable<SeriesColor> seriesColors);
  }
}

