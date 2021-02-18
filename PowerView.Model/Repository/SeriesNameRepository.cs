using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using Mono.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class SeriesNameRepository : RepositoryBase, ISeriesNameRepository
  {
    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SeriesNameRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public IList<SeriesName> GetSeriesNames(TimeZoneInfo timeZoneInfo)
    {
      var labelsAndObisCodes = GetLabelsAndObisCodes();

      var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
      var midnight = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, nowLocal.Kind).ToUniversalTime();
      var dt1 = midnight.AddMinutes(-10);
      var dt2 = midnight.AddMinutes(5);
      var fakeUnit = Unit.Joule;
      IEnumerable<TimeRegisterValue> fakeTimeRegisterValues = new List<TimeRegisterValue> { new TimeRegisterValue("1", dt1, 1, 0, fakeUnit), new TimeRegisterValue("1", dt2, 2, 0, fakeUnit) };

      var labelGroups = labelsAndObisCodes.GroupBy(x => (string)x.Label, x => (ObisCode)(long)x.ObisCode);
      var labelSeries = new List<LabelSeries<TimeRegisterValue>>(8);
      foreach (var labelToObisCodes in labelGroups)
      {
        var fakeObisToTimeRegisterValues = labelToObisCodes.Distinct().ToDictionary(oc => oc, x => fakeTimeRegisterValues);
        var labelS = new LabelSeries<TimeRegisterValue>(labelToObisCodes.Key, fakeObisToTimeRegisterValues);
        labelSeries.Add(labelS);
      }
      var labelSeriesSet = new LabelSeriesSet<TimeRegisterValue>(dt1, dt2, labelSeries);
      var intervalGroup = new IntervalGroup(timeZoneInfo, midnight, "5-minutes", labelSeriesSet);
      intervalGroup.Prepare();

      var seriesNames = intervalGroup.NormalizedLabelSeriesSet
        .SelectMany(ls => ls.Where(oc => !oc.IsCumulative).Select(oc => new SeriesName(ls.Label, oc)))
        .ToList();
      return seriesNames;
    }

    public IList<SeriesName> GetStoredSeriesNames()
    {
      var labelsAndObisCodes = GetLabelsAndObisCodes();

      var seriesNames = labelsAndObisCodes
        .Select(x => new SeriesName((string)x.Label, (long)x.ObisCode))
        .Distinct()
        .ToList();

      return seriesNames;
    }

    private IEnumerable<dynamic> GetLabelsAndObisCodes()
    {
      var transaction = DbContext.BeginTransaction();
      try
      {
        log.DebugFormat("Starting database operation GetSerieNames");
        var liveData = GetRecent<Db.LiveReading, Db.LiveRegister>(transaction, 1);
        var dayData = GetRecent<Db.DayReading, Db.DayRegister>(transaction, 60);
        var monthData = GetRecent<Db.MonthReading, Db.MonthRegister>(transaction, 700);

        transaction.Commit();
        log.DebugFormat("Finished database operation");

        return liveData.Concat(dayData).Concat(monthData);
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        log.DebugFormat("Finished database operation");
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    private IList<dynamic> GetRecent<TReading, TRegister>(IDbTransaction transaction, int cutoffDays) where TReading : IDbReading where TRegister : IDbRegister
    {
      var cutoffDateTime = DateTime.UtcNow - TimeSpan.FromDays(cutoffDays);

      string sqlQuery = @"
SELECT rea.Label AS Label, reg.ObisCode AS ObisCode
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp > @Cutoff
GROUP BY rea.Label, reg.ObisCode;";

      var readingTable = typeof(TReading).Name;
      var registerTable = typeof(TRegister).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      var resultSet = DbContext.Connection.Query(sqlQuery, new { Cutoff = cutoffDateTime }, transaction, buffered: false).ToList();

      return resultSet;
    }
  }

}
