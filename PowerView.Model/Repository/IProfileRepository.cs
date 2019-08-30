using System;

namespace PowerView.Model.Repository
{
  public interface IProfileRepository
  {
    LabelSeriesSet GetDayProfileSet(DateTime preStart, DateTime start, DateTime end);

    LabelSeriesSet GetMonthProfileSet(DateTime preStart, DateTime start, DateTime end);

    LabelSeriesSet GetYearProfileSet(DateTime preStart, DateTime start, DateTime end);

    LabelProfileSet GetDayProfileSet(DateTime start);

    LabelProfileSet GetMonthProfileSet(DateTime start);

    LabelProfileSet GetYearProfileSet(DateTime start);

    LabelProfileSet GetCustomProfileSet(DateTime from, DateTime to);
  }
}
