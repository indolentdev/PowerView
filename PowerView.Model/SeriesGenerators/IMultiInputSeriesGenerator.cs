using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public interface IMultiInputSeriesGenerator
  {
    bool IsSatisfiedBy(IDictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>> values);

    void CalculateNext(IDictionary<ObisCode, NormalizedDurationRegisterValue> obisCodeRegisterValues);

    IList<NormalizedDurationRegisterValue> GetGenerated();
  }
}
