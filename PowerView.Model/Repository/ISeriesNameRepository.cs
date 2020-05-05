using System;
using System.Collections.Generic;
using PowerView.Model.Expression;

namespace PowerView.Model.Repository
{
  public interface ISeriesNameRepository
  {
    IList<SeriesName> GetSeriesNames(TimeZoneInfo timeZoneInfo, ICollection<LabelObisCodeTemplate> labelObisCodeTemplates);

    IList<SeriesName> GetStoredSeriesNames();
  }
}

