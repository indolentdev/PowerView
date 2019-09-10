using System;

namespace PowerView.Model.Repository
{
  public interface ILabelSeriesRepository
  {
    LabelSeriesSet GetDayLabelSeriesSet(DateTime from, DateTime start, DateTime end);
  }
}
