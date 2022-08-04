using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dapper;
using System.Data.SQLite;

namespace PowerView.Model.Repository
{
  internal class GaugeRepository : RepositoryBase, IGaugeRepository
  {
    public GaugeRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public ICollection<GaugeValueSet> GetLatest(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

      var result = new List<GaugeValueSet>(4);

      var transaction = DbContext.BeginTransaction();
      try
      {
        GetLatestGaugeValueSet<Db.LiveReading, Db.LiveRegister>(transaction, GaugeSetName.Latest, dateTime, 2, result);
        GetLatestGaugeValueSet<Db.DayReading, Db.DayRegister>(transaction, GaugeSetName.LatestDay, dateTime, 14, result);
        GetLatestGaugeValueSet<Db.MonthReading, Db.MonthRegister>(transaction, GaugeSetName.LatestMonth, dateTime, 180, result);
        GetLatestGaugeValueSet<Db.YearReading, Db.YearRegister>(transaction, GaugeSetName.LatestYear, dateTime, 2*365, result);

        transaction.Commit();
      }
      catch (SQLiteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      return result;
    }

    /// <summary>
    /// Initial tests revealed that GROUP BY on these tables and indexes was way too slow in the database.
    /// Therefore the query uses the db index, and the group by is performed in memory...  :o
    /// </summary>
    private void GetLatestGaugeValueSet<TReading, TRegister>(IDbTransaction transaction, GaugeSetName name, DateTime dateTime, int cutoffDays, List<GaugeValueSet> result) where TReading : IDbReading where TRegister : IDbRegister
    {
      var cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

      var sqlQuery = @"
SELECT rea.Label,rea.DeviceId,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp > @Cutoff
ORDER BY rea.Timestamp DESC;";
      var readingTable = typeof(TReading).Name;
      var registerTable = typeof(TRegister).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      var resultSet = DbContext.Connection.Query(sqlQuery, new { Cutoff = cutoffDateTime }, transaction, buffered: true);

      var values = resultSet.Select(GetObisCode).Where(x => x.Item1.IsCumulative)
                            .Select(ToGaugeValue).GroupBy(gv => new { gv.Label, gv.ObisCode, gv.DeviceId })
                            .Select(x => x.First()).ToArray();

      if (values.Length > 0)
      {
        result.Add(new GaugeValueSet(name, values));
      }
    }

    public ICollection<GaugeValueSet> GetCustom(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

      var result = new List<GaugeValueSet>(4);

      var transaction = DbContext.BeginTransaction();
      try
      {
        GetCustomGaugeValueSet<Db.DayReading, Db.DayRegister>(transaction, GaugeSetName.Custom, dateTime, 2, result);
        transaction.Commit();
      }
      catch (SQLiteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      return result;
    }

    /// <summary>
    /// Initial tests revealed that GROUP BY on these tables and indexes was way too slow in the database.
    /// Therefore the query uses the db index, and the group by is performed in memory...  :o
    /// </summary>
    private void GetCustomGaugeValueSet<TReading, TRegister>(IDbTransaction transaction, GaugeSetName name, DateTime dateTime, int cutoffDays, List<GaugeValueSet> result) where TReading: IDbReading where TRegister: IDbRegister
    {
      var cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

      var sqlQuery = @"
SELECT rea.Label,rea.DeviceId,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp > @Cutoff AND rea.Timestamp < @dateTime
ORDER BY rea.Timestamp DESC;";
      var readingTable = typeof(TReading).Name;
      var registerTable = typeof(TRegister).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      var resultSet = DbContext.Connection.Query(sqlQuery, new { Cutoff = cutoffDateTime, dateTime }, transaction, buffered: true);

      var values = resultSet.Select(GetObisCode).Where(x => x.Item1.IsCumulative)
                            .Select(ToGaugeValue).GroupBy(gv => new { gv.Label, gv.ObisCode, gv.DeviceId })
                            .Select(x => x.First()).ToArray();

      if (values.Length > 0)
      {
        result.Add(new GaugeValueSet(name, values));
      }
    }

    private Tuple<ObisCode, dynamic> GetObisCode(dynamic row)
    { 
      var obisCode = (ObisCode)row.ObisCode;
      return new Tuple<ObisCode, dynamic>(obisCode, row);
    }

    private GaugeValue ToGaugeValue(Tuple<ObisCode,dynamic> r)
    {
      dynamic row = r.Item2;  
      var label = (string)row.Label;
      var deviceId = (string)row.DeviceId;
      var dateTime = (DateTime)row.Timestamp;
      var unitValue = new UnitValue((int)row.Value, (short)row.Scale, (Unit)row.Unit);
      return new GaugeValue(label, deviceId, dateTime, r.Item1, unitValue);
    }
  }
}
