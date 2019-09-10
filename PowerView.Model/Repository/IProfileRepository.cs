using System;

namespace PowerView.Model.Repository
{
  public interface IProfileRepository
  {
    LabelSeriesSet<TimeRegisterValue> GetDayProfileSet(DateTime preStart, DateTime start, DateTime end);

    LabelSeriesSet<TimeRegisterValue> GetMonthProfileSet(DateTime preStart, DateTime start, DateTime end);

    LabelSeriesSet<TimeRegisterValue> GetYearProfileSet(DateTime preStart, DateTime start, DateTime end);
  }
}
