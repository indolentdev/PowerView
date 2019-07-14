﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using DapperExtensions;
using log4net;

namespace PowerView.Model.Repository
{
  internal class DisconnectRuleRepository : RepositoryBase, IDisconnectRuleRepository
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DisconnectRuleRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public void AddDisconnectRule(DisconnectRule disconnectRule)
    { // INSERT (..) ON CONFLICT (..) not supported on the used sqlite3.. Added in 3.24
      if (disconnectRule == null) throw new ArgumentNullException("disconnectRule");

      var transaction = DbContext.BeginTransaction();
      try
      {
        var predicateLabel = Predicates.Field<Db.DisconnectRule>(x => x.Label, Operator.Eq, disconnectRule.Name.Label);
        var predicateObisCode = Predicates.Field<Db.DisconnectRule>(x => x.ObisCode, Operator.Eq, (long)disconnectRule.Name.ObisCode);
        var predicate = Predicates.Group(GroupOperator.And, predicateLabel, predicateObisCode);
        var dbDisconnectRule = DbContext.Connection.GetList<Db.DisconnectRule>(predicate, null, transaction).SingleOrDefault();
        if (dbDisconnectRule != null)
        {
          MapNonKeyValues(disconnectRule, dbDisconnectRule);
          DbContext.Connection.Update(dbDisconnectRule, transaction);
        }
        else
        {
          dbDisconnectRule = new Db.DisconnectRule { Label = disconnectRule.Name.Label, ObisCode = disconnectRule.Name.ObisCode };
          MapNonKeyValues(disconnectRule, dbDisconnectRule);
          DbContext.Connection.Insert(dbDisconnectRule, transaction);
        }
        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void DeleteDisconnectRule(ISerieName name)
    {
      if (name == null) throw new ArgumentNullException("name");

      const string sql = "DELETE FROM DisconnectRule WHERE Label=@Label AND ObisCode=@ObisCode;";
      DbContext.ExecuteTransaction("DeleteDisconnectRule", sql, new { name.Label, ObisCode = (long)name.ObisCode });
    }

    private static void MapNonKeyValues(DisconnectRule disconnectRule, Db.DisconnectRule dbDisconnectRule)
    {
      dbDisconnectRule.EvaluationLabel = disconnectRule.EvaluationName.Label;
      dbDisconnectRule.EvaluationObisCode = disconnectRule.EvaluationName.ObisCode;
      dbDisconnectRule.DurationSeconds = (int)disconnectRule.Duration.TotalSeconds;
      dbDisconnectRule.DisconnectToConnectValue = disconnectRule.DisconnectToConnectValue;
      dbDisconnectRule.ConnectToDisconnectValue = disconnectRule.ConnectToDisconnectValue;
      dbDisconnectRule.Unit = (byte)disconnectRule.Unit;
    }

    public ICollection<IDisconnectRule> GetDisconnectRules()
    {
      var sql = @"SELECT Id,Label,ObisCode,EvaluationLabel,EvaluationObisCode,DurationSeconds,DisconnectToConnectValue,ConnectToDisconnectValue,Unit FROM DisconnectRule ORDER BY Id;";
      
      var queryResult = DbContext.QueryTransaction<Db.DisconnectRule>("GetDisconnectRules", sql);

      return queryResult.Select(ToDisconnectRule).ToList();
    }

    private IDisconnectRule ToDisconnectRule(Db.DisconnectRule dr)
    {
      return new DisconnectRule(new SerieName(dr.Label, dr.ObisCode), new SerieName(dr.EvaluationLabel, dr.EvaluationObisCode), 
                                TimeSpan.FromSeconds(dr.DurationSeconds), dr.DisconnectToConnectValue, dr.ConnectToDisconnectValue, (Unit)dr.Unit);
    }

    public IDictionary<ISerieName, Unit> GetLatestSerieNames(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

      var latestLive = GetLatestRegisters<Db.LiveReading, Db.LiveRegister>(dateTime, 2);
      var latestDay = GetLatestRegisters<Db.DayReading, Db.DayRegister>(dateTime, 7);

      var serieNameToUnit = latestLive.Concat(latestDay)
                        .Select(x => new { Label = (string)x.Label, ObisCode = (long)x.ObisCode, Timestamp = (DateTime)x.Timestamp, Unit = (byte)x.Unit })
                        .Where(x => Enum.IsDefined(typeof(Unit), x.Unit))
                        .GroupBy(x => new { x.Label, x.ObisCode })
                        .Select(x => x.OrderByDescending(i => i.Timestamp).First())
                        .ToDictionary(x => (ISerieName)new SerieName(x.Label, x.ObisCode), x => (Unit)x.Unit);
      return serieNameToUnit;
    }

    private IList<dynamic> GetLatestRegisters<TReading, TRegister>(DateTime dateTime, int cutoffDays) where TReading : IDbReading where TRegister : IDbRegister
    {
      var cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

      var sqlQuery = @"
SELECT rea.Label,rea.Timestamp,reg.ObisCode,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp > @Cutoff
ORDER BY rea.Timestamp DESC;";
      var readingTable = typeof(TReading).Name;
      var registerTable = typeof(TRegister).Name;
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

      return DbContext.QueryTransaction<dynamic>("GetLatestRegisterValues-" + readingTable, sqlQuery, new { Cutoff = cutoffDateTime });
    }

  }
}