
namespace PowerView.Model.Repository
{
    public interface IReadingHistoryRepository
    {
        public void ClearDayMonthYearHistory();

        IList<(string Interval, IList<(string Label, DateTime LatestTimestamp)> Status)> GetReadingHistoryStatus();
    }
}
