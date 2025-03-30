using Dapper;
using Microsoft.Data.Sqlite;

namespace PowerView.Model.Repository
{
    internal class CrudeDataRepository : RepositoryBase, ICrudeDataRepository
    {
        private readonly ILocationContext locationContext;

        public CrudeDataRepository(IDbContext dbContext, ILocationContext locationContext)
          : base(dbContext)
        {
            this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
        }

        public WithCount<ICollection<CrudeDataValue>> GetCrudeData(string label, DateTime from, int skip = 0, int take = 10000)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgCheck.ThrowIfNotUtc(from);
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), $"Must be zero or greater. Was:{skip}");
            if (take < 1 || take > 25000) throw new ArgumentOutOfRangeException(nameof(take), $"Must be between 1 and 10000. Was:{take}");

            IEnumerable<RowLocal> resultSet;
            int totalCount;

            var sql = @"
SELECT rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit,dev.DeviceName AS DeviceId,regTag.Tags
FROM LiveReading AS rea 
JOIN Label AS lbl ON rea.LabelId=lbl.Id 
JOIN Device AS dev ON rea.DeviceId=dev.Id 
JOIN LiveRegister AS reg ON rea.Id=reg.ReadingId 
JOIN Obis o ON reg.ObisId=o.Id
LEFT OUTER JOIN LiveRegisterTag AS regTag ON reg.ReadingId=regTag.ReadingId AND reg.ObisId=regTag.ObisId
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
                .Select(x => new CrudeDataValue(x.Timestamp, x.ObisCode, x.Value, x.Scale, (Unit)x.Unit, x.DeviceId, x.Tags != null ? (RegisterValueTag)x.Tags : RegisterValueTag.None))
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
            public byte? Tags { get; set; }
        }

        public CrudeDataValue GetCrudeDataBy(string label, DateTime timestamp, ObisCode obisCode)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgCheck.ThrowIfNotUtc(timestamp);

            var sql = @"
SELECT rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit,dev.DeviceName AS DeviceId,regTag.Tags
FROM LiveReading AS rea 
JOIN Label AS lbl ON rea.LabelId=lbl.Id 
JOIN Device AS dev ON rea.DeviceId=dev.Id 
JOIN LiveRegister AS reg ON rea.Id=reg.ReadingId 
JOIN Obis o ON reg.ObisId=o.Id
LEFT OUTER JOIN LiveRegisterTag AS regTag ON reg.ReadingId=regTag.ReadingId AND reg.ObisId=regTag.ObisId
WHERE lbl.LabelName = @Label AND rea.Timestamp = @Timestamp AND o.ObisCode = @ObisCode;";

            var resultSet = DbContext.QueryTransaction<RowLocal>(sql, new { Label = label, Timestamp = (UnixTime)timestamp, ObisCode = (long)obisCode });

            var crudeDataValues = resultSet
                .Select(x => new CrudeDataValue(x.Timestamp, x.ObisCode, x.Value, x.Scale, (Unit)x.Unit, x.DeviceId, x.Tags != null ? (RegisterValueTag)x.Tags : RegisterValueTag.None))
                .ToList();

            return crudeDataValues.FirstOrDefault();
        }

        public void DeleteCrudeData(string label, DateTime timestamp, ObisCode obisCode)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgCheck.ThrowIfNotUtc(timestamp);

            var sql = @"
CREATE TEMP TABLE DeleteCrudeData AS 
SELECT rea.Id FROM LiveReading rea JOIN Label lbl ON rea.LabelId = lbl.Id
WHERE rea.Timestamp = @Timestamp AND lbl.LabelName = @Label;

DELETE FROM LiveRegisterTag WHERE ReadingID IN (SELECT Id FROM DeleteCrudeData) AND ObisId IN (SELECT o.Id FROM Obis o WHERE o.ObisCode = @ObisCode);

DELETE FROM LiveRegister WHERE ReadingID IN (SELECT Id FROM DeleteCrudeData) AND ObisId IN (SELECT o.Id FROM Obis o WHERE o.ObisCode = @ObisCode);

DELETE FROM LiveReading WHERE Id IN (SELECT Id FROM DeleteCrudeData) AND (1-EXISTS(SELECT 1 FROM LiveRegister reg JOIN DeleteCrudeData d ON reg.ReadingId = d.Id));
DROP TABLE DeleteCrudeData;";

            DbContext.ExecuteTransaction(sql, new { Label = label, Timestamp = (UnixTime)timestamp, ObisCode = (long)obisCode });
        }

        public IList<MissingDate> GetMissingDays(string label, ObisCode obisCode)
        {
            ArgumentNullException.ThrowIfNull(label);

            var readingInfo = GetReadingInfo(label, obisCode);
            var dayReadingInfo = readingInfo
              .Select(x => new { LocalDateTime = locationContext.ConvertTimeFromUtc(x.Timestamp), ReadingInfo = x })
              .GroupBy(x => DateOnly.FromDateTime(x.LocalDateTime))
              .Select(x => new { Date = x.Key, ReadingInfos = x.Select(y => y.ReadingInfo).OrderBy(x => x.Timestamp).ToList() })
              .OrderBy(x => x.Date)
              .ToList();

            if (dayReadingInfo.Count < 2)
            {
                return Array.Empty<MissingDate>();
            }

            var missingDays = new List<MissingDate>();

            var minItem = dayReadingInfo[0];
            var maxItem = dayReadingInfo[dayReadingInfo.Count - 1];
            var date = minItem.Date;
            var ix = 1;
            while (date < maxItem.Date)
            {
                date = date.AddDays(1);

                var item = dayReadingInfo[ix];
                if (date == item.Date)
                {
                    ix++;
                    continue;
                }

                var previousReadingInfo = dayReadingInfo[ix - 1].ReadingInfos.Last();
                var nextReadingInfo = dayReadingInfo[ix].ReadingInfos.First();
                if (!DeviceId.Equals(previousReadingInfo.DeviceId, nextReadingInfo.DeviceId))
                {  // No support for adding readings between meter exchanges.
                    continue;
                }

                var dateTime = date.ToDateTime(new TimeOnly(23, 59, 59));
                var dateTimeUtc = locationContext.ConvertTimeToUtc(dateTime);

                var missingDay = new { Date = date, Prev = previousReadingInfo, Next = nextReadingInfo };
                missingDays.Add(new MissingDate(dateTimeUtc, previousReadingInfo.Timestamp, nextReadingInfo.Timestamp));
            }

            return missingDays;
        }

        private record ReadingInfo(long Id, string DeviceId, UnixTime Timestamp)
        {
            public ReadingInfo() : this(0, string.Empty, new UnixTime(0)) {}
        }

        private IEnumerable<ReadingInfo> GetReadingInfo(string label, ObisCode obisCode)
        {
            IEnumerable<ReadingInfo> rows = Array.Empty<ReadingInfo>();

            long lastId = 0;
            var limit = 200000;
            var read = true;
            while (read)
            {
                var sql = @"
SELECT rea.Id, DeviceId, rea.Timestamp
FROM LiveReading rea
JOIN Label l on l.Id = rea.LabelId
JOIN LiveRegister reg on rea.Id = reg.ReadingId
join Obis o on reg.ObisId = o.Id
WHERE l.LabelName = @label AND rea.Id > @lastId AND o.ObisCode = @obisCode
ORDER BY rea.Id
LIMIT @limit";
                var page = DbContext.QueryTransaction<ReadingInfo>(sql, new { label, lastId, limit, obisCode = (long)obisCode });
                rows = rows.Concat(page);
                lastId = page[page.Count - 1].Id;

                if (page.Count < limit)
                {
                    read = false;
                }
            }

            return rows;
        }
    }
}
