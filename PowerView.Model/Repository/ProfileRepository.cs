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
  internal class ProfileRepository : RepositoryBase, IProfileRepository
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ProfileRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public LabelProfileSet GetDayProfileSet(DateTime start)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");

      var from = start.Subtract(TimeSpan.FromMinutes(30));
      var to = start.AddDays(1);

      return GetLabelProfileSet(start, from, to, "LiveReading", "LiveRegister");
    }

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

    public LabelProfileSet GetCustomProfileSet(DateTime from, DateTime to)
    {
      if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("from", "Must be UTC");
      if (to.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("to", "Must be UTC");

      var queryFrom = from.Subtract(TimeSpan.FromHours(12));

      return GetLabelProfileSet(from, queryFrom, to, "DayReading", "DayRegister");
    }

    private LabelProfileSet GetLabelProfileSet(DateTime start, DateTime from, DateTime to, string readingTable, string registerTable)
    {
      if (log.IsDebugEnabled) log.DebugFormat("Getting LabelProfileSet using start:{0}, from:{1}, to:{2}",
        start.ToString(CultureInfo.InvariantCulture), from.ToString(CultureInfo.InvariantCulture), to.ToString(CultureInfo.InvariantCulture));

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
        resultSet = DbContext.Connection.Query(sqlQuery, new { To = to, From = from }, transaction, buffered: true);
        transaction.Commit();
        log.Debug("Finished query");
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      var labelProfiles = GetLabelProfiles(start, resultSet);

      log.DebugFormat("Assembeled LabelProfileSet args");
      return new LabelProfileSet(start, labelProfiles);
    }

    private static List<LabelProfile> GetLabelProfiles(DateTime start, IEnumerable<dynamic> resultSet)
    {
      var labelProfiles = new List<LabelProfile>(5);
      var groupedByLabel = resultSet.GroupBy(r => { string s = r.Label; return s; }, r => r);
      foreach (IGrouping<string, dynamic> labelGroup in groupedByLabel)
      {
        var obisCodeToTimeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>();
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
        labelProfiles.Add(new LabelProfile(labelGroup.Key, start, obisCodeToTimeRegisterValues));
      }
      return labelProfiles;
    }

    private static DateTime NextMonth(DateTime date)
    {
      return date.Day != DateTime.DaysInMonth(date.Year, date.Month) ? date.AddMonths(1) : date.AddDays(1).AddMonths(1).AddDays(-1);
    }

  }
}
