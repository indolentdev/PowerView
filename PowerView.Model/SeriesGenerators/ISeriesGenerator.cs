using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public interface ISeriesGenerator
  {
    void CalculateNext(NormalizedTimeRegisterValue timeRegisterValue);

    IList<NormalizedTimeRegisterValue> GetGenerated();
  }
}
