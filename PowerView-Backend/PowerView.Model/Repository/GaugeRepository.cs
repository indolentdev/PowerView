using System.Data;
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;

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

            using var transaction = DbContext.BeginTransaction();
            try
            {
                GetLatestGaugeValueSet<Db.LiveReading, Db.LiveRegister>(transaction, GaugeSetName.Latest, dateTime, 2, result);
                GetLatestGaugeValueSet<Db.DayReading, Db.DayRegister>(transaction, GaugeSetName.LatestDay, dateTime, 14, result);
                GetLatestGaugeValueSet<Db.MonthReading, Db.MonthRegister>(transaction, GaugeSetName.LatestMonth, dateTime, 180, result);
                GetLatestGaugeValueSet<Db.YearReading, Db.YearRegister>(transaction, GaugeSetName.LatestYear, dateTime, 2 * 365, result);

                transaction.Commit();
            }
            catch (SqliteException e)
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
            UnixTime cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

            var sqlQuery = @"
SELECT lbl.LabelName AS Label,dev.DeviceName AS DeviceId,rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN Device AS dev ON rea.DeviceId=dev.Id JOIN {1} AS reg ON rea.Id=reg.ReadingId JOIN Obis o ON reg.ObisId=o.Id
WHERE rea.Timestamp > @Cutoff
ORDER BY rea.Timestamp DESC;";
            var readingTable = typeof(TReading).Name;
            var registerTable = typeof(TRegister).Name;
            sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

            var resultSet = DbContext.Connection.Query<RowLocal>(sqlQuery, new { Cutoff = cutoffDateTime }, transaction, buffered: true);

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

            using var transaction = DbContext.BeginTransaction();
            try
            {
                GetCustomGaugeValueSet<Db.DayReading, Db.DayRegister>(transaction, GaugeSetName.Custom, dateTime, 2, result);
                transaction.Commit();
            }
            catch (SqliteException e)
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
        private void GetCustomGaugeValueSet<TReading, TRegister>(IDbTransaction transaction, GaugeSetName name, DateTime dateTime, int cutoffDays, List<GaugeValueSet> result) where TReading : IDbReading where TRegister : IDbRegister
        {
            UnixTime cutoffDateTime = dateTime - TimeSpan.FromDays(cutoffDays);

            var sqlQuery = @"
SELECT lbl.LabelName AS Label,dev.DeviceName AS DeviceId,rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN Device AS dev ON rea.DeviceId=dev.Id JOIN {1} AS reg ON rea.Id=reg.ReadingId JOIN Obis o ON reg.ObisId=o.Id
WHERE rea.Timestamp > @Cutoff AND rea.Timestamp < @dateTime
ORDER BY rea.Timestamp DESC;";
            var readingTable = typeof(TReading).Name;
            var registerTable = typeof(TRegister).Name;
            sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);

            var resultSet = DbContext.Connection.Query<RowLocal>(sqlQuery, new { Cutoff = cutoffDateTime, dateTime = (UnixTime)dateTime }, transaction, buffered: true);

            var values = resultSet.Select(GetObisCode).Where(x => x.ObisCode.IsCumulative)
                                  .Select(ToGaugeValue).GroupBy(gv => new { gv.Label, gv.ObisCode, gv.DeviceId })
                                  .Select(x => x.First()).ToArray();

            if (values.Length > 0)
            {
                result.Add(new GaugeValueSet(name, values));
            }
        }

        private (ObisCode ObisCode, RowLocal Row) GetObisCode(RowLocal row)
        {
            var obisCode = (ObisCode)row.ObisCode;
            return  (obisCode, row);
        }

        private GaugeValue ToGaugeValue((ObisCode ObisCode, RowLocal Row) r)
        {
            var unitValue = new UnitValue(r.Row.Value, r.Row.Scale, (Unit)r.Row.Unit);
            return new GaugeValue(r.Row.Label, r.Row.DeviceId, r.Row.Timestamp, r.ObisCode, unitValue);
        }

        private class RowLocal
        {
            public string Label { get; set; }
            public string DeviceId { get; set; }
            public UnixTime Timestamp { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }
    }
}
