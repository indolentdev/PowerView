using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerView.Model.Repository
{
    internal class ReadingHistoryRepository : RepositoryBase, IReadingHistoryRepository
    {
        public ReadingHistoryRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public void ClearDayMonthYearHistory()
        {
            DbContext.ExecuteTransaction(@"
DELETE FROM YearRegister;
DELETE FROM YearReading;
DELETE FROM MonthRegister;
DELETE FROM MonthReading;            
DELETE FROM DayRegister;
DELETE FROM DayReading;
DELETE FROM StreamPosition;");
        }

        public IList<(string Interval, IList<(string Label, DateTime LatestTimestamp)> Status)> GetReadingHistoryStatus()
        {
            var dayReading = GetPipeStatus("DayReading", "LiveReading");
            var monthReading = GetPipeStatus("MonthReading", "DayReading");
            var yearReading = GetPipeStatus("YearReading", "MonthReading");

            var result = new List<(string Interval, IList<(string Label, DateTime LatestTimestamp)> Status)>();

            result.Add((dayReading.Interval, dayReading.Status));
            result.Add((monthReading.Interval, monthReading.Status));
            result.Add((yearReading.Interval, yearReading.Status));

            return result;
        }

        private (string Interval, IList<(string Label, DateTime LatestTimestamp)> Status) GetPipeStatus(string streamName, string readingTableName)
        {
            var sql = @$"
SELECT lbl.LabelName AS Label, rea.Timestamp
FROM StreamPosition sp
JOIN Label lbl ON sp.LabelId=lbl.Id
JOIN {readingTableName} rea ON sp.Position = rea.Id
WHERE sp.StreamName=@streamName;";

            var rows = DbContext.QueryTransaction<RowLocal>(sql, new { streamName });

            return (streamName.Replace("Reading", string.Empty), rows.Select(x => (Label: x.Label, LatestTimestamp: x.Timestamp.ToDateTime())).ToList());
        }

        private class RowLocal
        {
            public string Label { get; set; }
            public UnixTime Timestamp { get; set; }
        }

    }
}
