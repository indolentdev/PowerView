using Dapper;
using Microsoft.Data.Sqlite;

namespace PowerView.Model.Repository
{
    internal class CrudeDataRepository : RepositoryBase, ICrudeDataRepository
    {
        public CrudeDataRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public WithCount<ICollection<CrudeDataValue>> GetCrudeData(string label, DateTime from, int skip = 0, int take = 3000)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(from), $"Must be UTC. Was:{from.Kind}");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), $"Must be zero or greater. Was:{skip}");
            if (take < 1 || take > 10000) throw new ArgumentOutOfRangeException(nameof(take), $"Must be between 1 and 10000. Was:{take}");

            IEnumerable<RowLocal> resultSet;
            int totalCount;

            var sql = @"
SELECT rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit,dev.DeviceName AS DeviceId 
FROM LiveReading AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN Device AS dev ON rea.DeviceId=dev.Id JOIN LiveRegister AS reg ON rea.Id=reg.ReadingId JOIN Obis o ON reg.ObisId=o.Id
WHERE lbl.LabelName = @Label AND rea.Timestamp >= @From
ORDER BY rea.Timestamp ASC, o.ObisCode ASC
LIMIT @Take OFFSET @Skip;";

            var sqlTotalCount = @"
SELECT count(*)  
FROM LiveReading AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN LiveRegister AS reg ON rea.Id=reg.ReadingId
WHERE lbl.LabelName = @Label AND rea.Timestamp >= @From;";
            using (var transaction = DbContext.BeginTransaction())
            {
                try
                {
                    resultSet = DbContext.Connection.Query<RowLocal>(sql, new { Label = label, From = (UnixTime)from, Take = take, Skip = skip }, transaction, buffered: true);
                    totalCount = DbContext.Connection.ExecuteScalar<int>(sqlTotalCount, new { Label = label, From = (UnixTime)from }, transaction);

                    transaction.Commit();
                }
                catch (SqliteException e)
                {
                    transaction.Rollback();
                    throw DataStoreExceptionFactory.Create(e);
                }
            }

            var crudeDataValues = resultSet
                .Select(x => new CrudeDataValue(x.Timestamp, x.ObisCode, x.Value, x.Scale, (Unit)x.Unit, x.DeviceId))
                .ToList();

            return new WithCount<ICollection<CrudeDataValue>>(totalCount, crudeDataValues);            
        }

        private class RowLocal
        {
            public UnixTime Timestamp { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
            public string DeviceId { get; set; }
        }

    }
}
