using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dapper;
using Mono.Data.Sqlite;
using log4net;

namespace PowerView.Model.Repository
{
  internal class LabelSeriesRepository : RepositoryBase, ILabelSeriesRepository
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public LabelSeriesRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public LabelSeriesSet GetDayLabelSeriesSet(DateTime from, DateTime start, DateTime end)
    {
      if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("from", "Must be UTC");
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");

      return GetLabelSeriesSet(from, start, end, "LiveReading", "LiveRegister");
    }
/*
    public LabelProfileSet GetMonthProfileSet(DateTime start)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");

      var from = start.Subtract(TimeSpan.FromHours(12));
      var to = NextMonth(start).AddHours(1); // Also compensate for daylight saving

      return GetLabelProfileSet(start, from, to, "DayReading", "DayRegister");
    }

    public LabelProfileSet GetYearProfileSet(DateTime start)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");

      var from = start.Subtract(TimeSpan.FromDays(16));
      var to = start.AddMonths(12);

      return GetLabelProfileSet(start, from, to, "MonthReading", "MonthRegister");
    }
*/
/*
    public LabelProfileSet GetCustomProfileSet(DateTime from, DateTime to)
    {
      if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("from", "Must be UTC");
      if (to.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("to", "Must be UTC");

      var queryFrom = from.Subtract(TimeSpan.FromHours(12));

      return GetLabelProfileSet(from, queryFrom, to, "DayReading", "DayRegister");
    }
*/
    private LabelSeriesSet GetLabelSeriesSet(DateTime from, DateTime start, DateTime end, string readingTable, string registerTable)
    {
      if (log.IsDebugEnabled) log.DebugFormat("Getting LabelSeriesSet using from:{0}, start:{1}, end:{2}",
        from.ToString(CultureInfo.InvariantCulture), start.ToString(CultureInfo.InvariantCulture), end.ToString(CultureInfo.InvariantCulture));

      IEnumerable<dynamic> resultSet;

      var sqlQuery = @"
SELECT rea.Label,rea.SerialNumber,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp >= @From AND rea.Timestamp < @To;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);
      log.DebugFormat("Querying {0}", readingTable);
      var transaction = DbContext.BeginTransaction();
      try
      {
        resultSet = DbContext.Connection.Query(sqlQuery, new { To = end, From = from }, transaction, buffered: true);
        transaction.Commit();
        log.Debug("Finished query");
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      var labelSeries = GetLabelSeries(start, end, resultSet);

      log.DebugFormat("Assembeled LabelSeriesSet args");
      return new LabelSeriesSet(start, end, labelSeries);
    }

    private static List<LabelSeries> GetLabelSeries(DateTime start, DateTime end, IEnumerable<dynamic> resultSet)
    {
      var labelSeries = new List<LabelSeries>(5);
      var groupedByLabel = resultSet.GroupBy(r => { string s = r.Label; return s; }, r => r);
      foreach (IGrouping<string, dynamic> labelGroup in groupedByLabel)
      {
        var obisCodeToTimeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>();
        var groupedByObisCode = labelGroup.GroupBy(r => { ObisCode oc = r.ObisCode; return oc; }, r => r);
        foreach (IGrouping<ObisCode, dynamic> obisCodeGroup in groupedByObisCode)
        {
          var timeRegisterValues = new List<TimeRegisterValue>(130);
          foreach (dynamic row in obisCodeGroup)
          {
            timeRegisterValues.Add(new TimeRegisterValue((string)row.SerialNumber, (DateTime)row.Timestamp, (int)row.Value, (short)row.Scale, (Unit)row.Unit));
          }
          obisCodeToTimeRegisterValues.Add(obisCodeGroup.Key, timeRegisterValues);
        }
        labelSeries.Add(new LabelSeries(labelGroup.Key, obisCodeToTimeRegisterValues));
      }
      return labelSeries;
    }

    private static DateTime NextMonth(DateTime date)
    {
      return date.Day != DateTime.DaysInMonth(date.Year, date.Month) ? date.AddMonths(1) : date.AddDays(1).AddMonths(1).AddDays(-1);
    }

  }
}
