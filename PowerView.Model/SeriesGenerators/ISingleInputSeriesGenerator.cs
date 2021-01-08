using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public interface ISingleInputSeriesGenerator
  {
    void CalculateNext(NormalizedTimeRegisterValue timeRegisterValue);

    IList<NormalizedTimeRegisterValue> GetGenerated();
  }
}
