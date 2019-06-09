using System;

namespace PowerView.Model.Repository
{
  public interface IProfileRepository
  {
    LabelProfileSet GetDayProfileSet(DateTime start);

    LabelProfileSet GetMonthProfileSet(DateTime start);

    LabelProfileSet GetYearProfileSet(DateTime start);

    LabelProfileSet GetCustomProfileSet(DateTime from, DateTime to);
  }
}
