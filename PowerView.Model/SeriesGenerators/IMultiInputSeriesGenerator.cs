using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public interface IMultiInputSeriesGenerator
  {
    bool IsSatisfiedBy(IDictionary<ObisCode, IList<NormalizedTimeRegisterValue>> values);

    void CalculateNext(DateTime normalizedTimestamp, IDictionary<ObisCode, TimeRegisterValue> obisCodeRegisterValues);

    IList<NormalizedTimeRegisterValue> GetGenerated();
  }
}
