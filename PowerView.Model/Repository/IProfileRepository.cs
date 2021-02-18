using System;

namespace PowerView.Model.Repository
{
  public interface IProfileRepository
  {
    TimeRegisterValueLabelSeriesSet GetDayProfileSet(DateTime preStart, DateTime start, DateTime end);

    TimeRegisterValueLabelSeriesSet GetMonthProfileSet(DateTime preStart, DateTime start, DateTime end);

    TimeRegisterValueLabelSeriesSet GetYearProfileSet(DateTime preStart, DateTime start, DateTime end);
  }
}
