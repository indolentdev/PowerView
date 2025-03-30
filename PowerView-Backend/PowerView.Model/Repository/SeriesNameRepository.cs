using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class SeriesNameRepository : RepositoryBase, ISeriesNameRepository
  {
    public SeriesNameRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public IList<SeriesName> GetSeriesNames()
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
      using var transaction = DbContext.BeginTransaction();
      try
      {
        var liveData = GetRecent<Db.LiveReading, Db.LiveRegister>(transaction, 1);
        var dayData = GetRecent<Db.DayReading, Db.DayRegister>(transaction, 60);
        var monthData = GetRecent<Db.MonthReading, Db.MonthRegister>(transaction, 700);

        transaction.Commit();

        return liveData.Concat(dayData).Concat(monthData);
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    private List<dynamic> GetRecent<TReading, TRegister>(IDbTransaction transaction, int cutoffDays) where TReading : IDbReading where TRegister : IDbRegister
    {
      UnixTime cutoffDateTime = DateTime.UtcNow - TimeSpan.FromDays(cutoffDays);

      string sqlQuery = @"
SELECT lbl.LabelName AS Label, o.ObisCode AS ObisCode
FROM {0} AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN {1} AS reg ON rea.Id=reg.ReadingId JOIN Obis o ON reg.ObisId=o.Id
WHERE rea.Timestamp > @Cutoff
GROUP BY lbl.LabelName, o.ObisCode;";

      var readingTable = typeof(TReading).Name;
      var registerTable = typeof(TRegister).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      var resultSet = DbContext.Connection.Query(sqlQuery, new { Cutoff = cutoffDateTime }, transaction, buffered: false).ToList();

      return resultSet;
    }
  }

}
