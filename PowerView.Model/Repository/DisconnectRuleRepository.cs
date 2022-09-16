using System.Globalization;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class DisconnectRuleRepository : RepositoryBase, IDisconnectRuleRepository
    {
        public DisconnectRuleRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public void AddDisconnectRule(DisconnectRule disconnectRule)
        { // INSERT (..) ON CONFLICT (..) not supported on the used sqlite3.. Added in 3.24
            if (disconnectRule == null) throw new ArgumentNullException("disconnectRule");

            using var transaction = DbContext.BeginTransaction();
            try
            {
                var dbDisconnectRule = DbContext.Connection.QueryFirstOrDefault<Db.DisconnectRule>(
                  "SELECT Id,Label,ObisCode,EvaluationLabel,EvaluationObisCode,DurationSeconds,DisconnectToConnectValue,ConnectToDisconnectValue,Unit FROM DisconnectRule WHERE Label = @Label AND ObisCode = @ObisCode;",
                  new { disconnectRule.Name.Label, ObisCode = (long)disconnectRule.Name.ObisCode }, transaction);
                if (dbDisconnectRule != null)
                {
                    MapNonKeyValues(disconnectRule, dbDisconnectRule);
                    DbContext.Connection.Execute(
                      "UPDATE DisconnectRule SET EvaluationLabel=@EvaluationLabel,EvaluationObisCode=@EvaluationObisCode,DurationSeconds=@DurationSeconds,DisconnectToConnectValue=@DisconnectToConnectValue,ConnectToDisconnectValue=@ConnectToDisconnectValue,Unit=@Unit WHERE Id = @Id AND Label = @Label AND ObisCode = @ObisCode;",
                      dbDisconnectRule, transaction);
                }
                else
                {
                    dbDisconnectRule = new Db.DisconnectRule { Label = disconnectRule.Name.Label, ObisCode = disconnectRule.Name.ObisCode };
                    MapNonKeyValues(disconnectRule, dbDisconnectRule);
                    DbContext.Connection.Execute(
                      "INSERT INTO DisconnectRule (Label,ObisCode,EvaluationLabel,EvaluationObisCode,DurationSeconds,DisconnectToConnectValue,ConnectToDisconnectValue,Unit) VALUES (@Label,@ObisCode,@EvaluationLabel,@EvaluationObisCode,@DurationSeconds,@DisconnectToConnectValue,@ConnectToDisconnectValue,@Unit);",
                      dbDisconnectRule, transaction);
                }
                transaction.Commit();
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                throw DataStoreExceptionFactory.Create(e);
            }
        }

        public void DeleteDisconnectRule(ISeriesName name)
        {
            if (name == null) throw new ArgumentNullException("name");

            const string sql = "DELETE FROM DisconnectRule WHERE Label=@Label AND ObisCode=@ObisCode;";
            DbContext.ExecuteTransaction(sql, new { name.Label, ObisCode = (long)name.ObisCode });
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

            var queryResult = DbContext.QueryTransaction<Db.DisconnectRule>(sql);

            return queryResult.Select(ToDisconnectRule).ToList();
        }

        private IDisconnectRule ToDisconnectRule(Db.DisconnectRule dr)
        {
            return new DisconnectRule(new SeriesName(dr.Label, dr.ObisCode), new SeriesName(dr.EvaluationLabel, dr.EvaluationObisCode),
                                      TimeSpan.FromSeconds(dr.DurationSeconds), dr.DisconnectToConnectValue, dr.ConnectToDisconnectValue, (Unit)dr.Unit);
        }

        public IDictionary<ISeriesName, Unit> GetLatestSerieNames(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime");

            var latestLive = GetLatestRegisters<Db.LiveReading, Db.LiveRegister>(dateTime, 2);
            var latestDay = GetLatestRegisters<Db.DayReading, Db.DayRegister>(dateTime, 7);

            var serieNameToUnit = latestLive.Concat(latestDay)
                              .Where(x => Enum.IsDefined(typeof(Unit), x.Unit))
                              .GroupBy(x => new { x.Label, x.ObisCode })
                              .Select(x => x.OrderByDescending(i => i.Timestamp).First())
                              .ToDictionary(x => (ISeriesName)new SeriesName(x.Label, x.ObisCode), x => (Unit)x.Unit);
            return serieNameToUnit;
        }

        private IList<RowLocal> GetLatestRegisters<TReading, TRegister>(DateTime dateTime, int cutoffDays) where TReading : IDbReading where TRegister : IDbRegister
        {
            UnixTime cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

            var sqlQuery = @"
SELECT lbl.LabelName AS Label,rea.Timestamp,reg.ObisCode,reg.Unit 
FROM {0} AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp > @Cutoff
ORDER BY rea.Timestamp DESC;";
            var readingTable = typeof(TReading).Name;
            var registerTable = typeof(TRegister).Name;
            sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

            return DbContext.QueryTransaction<RowLocal>(sqlQuery, new { Cutoff = cutoffDateTime });
        }

        private class RowLocal
        {
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public UnixTime Timestamp { get; set; }
            public byte Unit { get; set; }
        }

    }
}
