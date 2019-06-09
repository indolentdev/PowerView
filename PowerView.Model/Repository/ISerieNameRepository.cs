using System.Collections.Generic;
using PowerView.Model.Expression;

namespace PowerView.Model.Repository
{
  public interface ISerieNameRepository
  {
    IList<SerieName> GetSerieNames(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates);
  }
}

