using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerView.Model.Repository
{
  internal class ProfileRepository : RepositoryBase, IProfileRepository
  {
    public ProfileRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public TimeRegisterValueLabelSeriesSet GetDayProfileSet(DateTime preStart, DateTime start, DateTime end)
    {
      return GetLabelSeriesSet(preStart, start, end, "LiveReading", "LiveRegister");
    }

    public TimeRegisterValueLabelSeriesSet GetMonthProfileSet(DateTime preStart, DateTime start, DateTime end)
    {
      return GetLabelSeriesSet(preStart, start, end, "DayReading", "DayRegister");
    }

    public TimeRegisterValueLabelSeriesSet GetYearProfileSet(DateTime preStart, DateTime start, DateTime end)
    {
      return GetLabelSeriesSet(preStart, start, end, "MonthReading", "MonthRegister");
    }

    private TimeRegisterValueLabelSeriesSet GetLabelSeriesSet(DateTime preStart, DateTime start, DateTime end, string readingTable, string registerTable)
    {
      if (preStart.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("preStart", "Must be UTC");
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");

      var sqlQuery = @"
SELECT rea.Label,rea.DeviceId,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp >= @From AND rea.Timestamp < @To;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      var resultSet = DbContext.QueryTransaction(sqlQuery, new { From = preStart, To = end });

      var labelSeries = GetLabelSeries(resultSet);

      return new TimeRegisterValueLabelSeriesSet(start, end, labelSeries);
    }

    private static List<TimeRegisterValueLabelSeries> GetLabelSeries(IEnumerable<dynamic> resultSet)
    {
      var labelSeries = new List<TimeRegisterValueLabelSeries>(5);
      var groupedByLabel = resultSet.GroupBy(r => { string s = r.Label; return s; }, r => r);
      foreach (IGrouping<string, dynamic> labelGroup in groupedByLabel)
      {
        var obisCodeToTimeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>(8);
        var groupedByObisCode = labelGroup.GroupBy(r => { ObisCode oc = r.ObisCode; return oc; }, r => r);
        foreach (IGrouping<ObisCode, dynamic> obisCodeGroup in groupedByObisCode)
        {
          obisCodeToTimeRegisterValues.Add(obisCodeGroup.Key, obisCodeGroup.Select(row =>
            new TimeRegisterValue((string)row.DeviceId, (DateTime)row.Timestamp, (int)row.Value, (short)row.Scale, (Unit)row.Unit)) );
        }
        labelSeries.Add(new TimeRegisterValueLabelSeries(labelGroup.Key, obisCodeToTimeRegisterValues));
      }
      return labelSeries;
    }

    private static DateTime NextMonth(DateTime date)
    {
      return date.Day != DateTime.DaysInMonth(date.Year, date.Month) ? date.AddMonths(1) : date.AddDays(1).AddMonths(1).AddDays(-1);
    }

  }
}
