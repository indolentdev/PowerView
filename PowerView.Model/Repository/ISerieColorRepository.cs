using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface ISerieColorRepository
  {
    string GetColorCached(string label, ObisCode obisCode);

    ICollection<SerieColor> GetSerieColors();

    void SetSerieColors(IEnumerable<SerieColor> serieColors);
  }
}

