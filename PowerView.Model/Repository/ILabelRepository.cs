using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface ILabelRepository
  {
    IList<string> GetLabels();
  }
}
