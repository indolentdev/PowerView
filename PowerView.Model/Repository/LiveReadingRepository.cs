using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class LiveReadingRepository : RepositoryBase, ILiveReadingRepository
    {
        public LiveReadingRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public void Add(IList<Reading> liveReadings)
        {
            if (liveReadings == null) throw new ArgumentNullException("liveReadings");
            if (liveReadings.Any(lr => lr == null)) throw new ArgumentOutOfRangeException("liveReadings", "Must not contain nulls");

            if (liveReadings.Count == 0)
            {
                return;
            }

            var labels = GetLabelIds(liveReadings.Select(l => l.Label).Distinct().ToList())
                .ToDictionary(x => x.Label, x => x.Id);
            var deviceIds = GetDeviceIds(liveReadings.Select(l => l.DeviceId).Distinct().ToList())
                .ToDictionary(x => x.DeviceId, x => x.Id);
            var obisIds = GetObisIds(liveReadings.SelectMany(x => x.GetRegisterValues()).Select(x => x.ObisCode).Distinct().ToList())
                .ToDictionary(x => x.ObisCode, x => x.Id);

            var dbReadingsMap = liveReadings.ToDictionary(lr => new Db.LiveReading { LabelId = labels[lr.Label], DeviceId = deviceIds[lr.DeviceId], Timestamp = lr.Timestamp });
            using var transaction = DbContext.BeginTransaction();
            try
            {
                // First insert the readings
                foreach (var reading in dbReadingsMap.Keys)
                {
                    var readingId = DbContext.Connection.QueryFirstOrDefault<long?>(
                      "INSERT OR IGNORE INTO LiveReading (LabelId, DeviceId, Timestamp) VALUES (@LabelId, @DeviceId, @Timestamp); SELECT Id FROM LiveReading WHERE Timestamp=@Timestamp AND LabelId=@LabelId;",
                      reading, transaction);

                    if (readingId == null || readingId == 0)
                    {
                        throw new SqliteException($"Reading.Id not provided after insert. Timestamp:{((DateTime)reading.Timestamp).ToString("o")}, LabelId:{reading.LabelId}, Reading.Id:{readingId}", 28); // 28: SQLITE Warning
                    }

                    // remember the Reading.Id value for the reading, for foreign key composition.
                    reading.Id = readingId.Value;
                }

                // then insert the registers
                var dbLiveRegisters = GetDbLiveRegisters(dbReadingsMap, obisIds).ToList();
                DbContext.Connection.Execute("INSERT INTO LiveRegister (ReadingId, ObisId, Value, Scale, Unit) VALUES (@ReadingId, @ObisId, @Value, @Scale, @Unit);",
                  dbLiveRegisters, transaction);

                // then insert the register tags
                var dbLiveRegisterTags = GetDbLiveRegisterTags(dbReadingsMap, obisIds).Where(x => x.Tags != 0).ToList();
                DbContext.Connection.Execute("INSERT INTO LiveRegisterTag (ReadingId, ObisId, Tags) VALUES (@ReadingId, @ObisId, @Tags);",
                  dbLiveRegisterTags, transaction);

                transaction.Commit();
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                var readingDetails = string.Join(", ", dbReadingsMap
                  .Select(x => $"(Label:{labels.FirstOrDefault(z => z.Value == x.Key.LabelId)} ({x.Key.LabelId}), Timestamp:{((DateTime)x.Key.Timestamp).ToString("o")}, Id:{x.Key.Id})"));
                throw DataStoreExceptionFactory.Create(e, $"Insert readings failed. Readings:{readingDetails}");
            }
        }

        private static IEnumerable<Db.LiveRegister> GetDbLiveRegisters(IDictionary<Db.LiveReading, Reading> dbLiveReadingsMap, IDictionary<ObisCode, byte> obisIds)
        {
            foreach (var entry in dbLiveReadingsMap)
            {
                foreach (var lr in entry.Value.GetRegisterValues())
                {
                    yield return new Db.LiveRegister { ReadingId = entry.Key.Id, ObisId = obisIds[lr.ObisCode], Value = lr.Value, Scale = lr.Scale, Unit = (byte)lr.Unit };
                }
            }
        }

        private static IEnumerable<Db.LiveRegisterTag> GetDbLiveRegisterTags(IDictionary<Db.LiveReading, Reading> dbLiveReadingsMap, IDictionary<ObisCode, byte> obisIds)
        {
            foreach (var entry in dbLiveReadingsMap)
            {
                foreach (var lr in entry.Value.GetRegisterValues())
                {
                    yield return new Db.LiveRegisterTag { ReadingId = entry.Key.Id, ObisId = obisIds[lr.ObisCode], Tags = (byte)lr.Tag };
                }
            }
        }

        private IList<(byte Id, string Label)> GetLabelIds(List<string> labels)
        {
            UnixTime now = DateTime.UtcNow;
            var labelsAndTimestamps = labels.Select(x => new { LabelName = x, Timestamp = now });
            DbContext.ExecuteTransaction(@"
              INSERT INTO Label (LabelName, Timestamp) VALUES (@LabelName, @Timestamp)
                ON CONFLICT(LabelName) DO UPDATE SET Timestamp = @Timestamp;", labelsAndTimestamps);

            return DbContext.QueryTransaction<(byte Id, string Label)>("SELECT Id, LabelName FROM Label;");
        }

        private IList<(byte Id, string DeviceId)> GetDeviceIds(List<string> deviceIds)
        {
            UnixTime now = DateTime.UtcNow;
            var deviceIdsAndTimestamps = deviceIds.Select(x => new { DeviceName = x, Timestamp = now });
            DbContext.ExecuteTransaction(@"
              INSERT INTO Device (DeviceName, Timestamp) VALUES (@DeviceName, @Timestamp)
                ON CONFLICT(DeviceName) DO UPDATE SET Timestamp = @Timestamp;", deviceIdsAndTimestamps);

            return DbContext.QueryTransaction<(byte Id, string DeviceId)>("SELECT Id, DeviceName FROM Device;");
        }

        private IList<(byte Id, ObisCode ObisCode)> GetObisIds(List<ObisCode> obisCodes)
        {
            var obisCodesLocal = obisCodes.Select(x => new { ObisCode = (long)x });
            DbContext.ExecuteTransaction(@"
              INSERT INTO Obis (ObisCode) VALUES (@ObisCode)
                ON CONFLICT(ObisCode) DO NOTHING;", obisCodesLocal);

            return DbContext.QueryTransaction<(byte Id, long ObisCode)>("SELECT Id, ObisCode FROM Obis;")
                .Select(x => (x.Id, (ObisCode)x.ObisCode))
                .ToList();
        }

    }
}
