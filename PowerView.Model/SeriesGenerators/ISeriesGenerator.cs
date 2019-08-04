using System;
using System.Collections.Generic;

namespace PowerView.Model.SeriesGenerators
{
  public interface ISeriesGenerator
  {
    void CalculateNext(TimeRegisterValue timeRegisterValue);

    IList<TimeRegisterValue> GetGenerated();
  }
}
