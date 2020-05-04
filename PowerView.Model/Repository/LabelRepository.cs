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
  internal class LabelRepository : RepositoryBase, ILabelRepository
  {
    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public LabelRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public IList<string> GetLabels()
    {
      var labels = GetLabelsDynamic().Select(x => (string)x.Label).Distinct().ToList();

      return labels;
    }

    private IEnumerable<dynamic> GetLabelsDynamic()
    {
      var transaction = DbContext.BeginTransaction();
      try
      {
        log.DebugFormat("Starting database operation GetLabels");
        var liveData = GetRecent<Db.LiveReading>(transaction, 1);
        var dayData = GetRecent<Db.DayReading>(transaction, 60);
        var monthData = GetRecent<Db.MonthReading>(transaction, 700);

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

    private IList<dynamic> GetRecent<TReading>(IDbTransaction transaction, int cutoffDays) where TReading : IDbReading
    {
      var cutoffDateTime = DateTime.UtcNow - TimeSpan.FromDays(cutoffDays);

      string sqlQuery = @"
SELECT rea.Label AS Label
FROM {0} AS rea
WHERE rea.Timestamp > @Cutoff
GROUP BY rea.Label;";

      var readingTable = typeof(TReading).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable);

      var resultSet = DbContext.Connection.Query(sqlQuery, new { Cutoff = cutoffDateTime }, transaction, buffered: false).ToList();

      return resultSet;
    }
  }

}
