using System.Collections.Generic;
using PowerView.Model.Expression;

namespace PowerView.Model.Repository
{
  public interface ISeriesNameRepository
  {
    IList<SeriesName> GetSeriesNames(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates);
  }
}

