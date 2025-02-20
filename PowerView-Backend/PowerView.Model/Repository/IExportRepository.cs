using System;
using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface IExportRepository
  {
    TimeRegisterValueLabelSeriesSet GetLiveCumulativeSeries(DateTime from, DateTime to, IList<string> labels);
  }
}
