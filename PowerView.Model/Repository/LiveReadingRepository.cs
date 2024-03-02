using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class LiveReadingRepository : RepositoryBase, ILiveReadingRepository
    {
        private readonly ILogger logger;

        public LiveReadingRepository(ILogger<LiveReadingRepository> logger, IDbContext dbContext)
          : base(dbContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var ignoredRegisters = new Dictionary<(long ReadingId, byte ObisId), Db.LiveRegister>();

            using var transaction = DbContext.BeginTransaction();
            try
            {
                // First insert the readings
                foreach (var readingItem in dbReadingsMap)
                {
                    var dbLabelObisCodes = DbContext.Connection.Query<long>("SELECT oc.ObisCode FROM LabelObisLive lol JOIN Label lbl ON lol.LabelId=lbl.Id JOIN Obis oc ON lol.ObisId=oc.Id WHERE lbl.LabelName=@Label;", readingItem.Value).Select(x => (ObisCode)x).ToList();
                    var readingObisCodes = readingItem.Value.GetRegisterValues().Select(x => x.ObisCode).ToList();
                    if (dbLabelObisCodes.Count > 0 && readingObisCodes.Count > 0 && dbLabelObisCodes.Contains(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat) != readingObisCodes.Contains(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat))
                    {
                        throw new SqliteException($"Unsupported obis code combination for label '{readingItem.Value.Label}'. Db ObisCodes:{string.Join(", ", dbLabelObisCodes)}. Reading obis codes:{string.Join(", ", readingObisCodes)}", 28); // 28: SQLITE Warning
                    }

                    var readingsAffected = DbContext.Connection.Execute("INSERT OR IGNORE INTO LiveReading (LabelId, DeviceId, Timestamp) VALUES (@LabelId, @DeviceId, @Timestamp);", readingItem.Key, transaction);
                    var readingId = DbContext.Connection.QueryFirstOrDefault<long?>("SELECT Id FROM LiveReading WHERE Timestamp=@Timestamp AND LabelId=@LabelId;", readingItem.Key, transaction);

                    if (readingId == null || readingId == 0)
                    {
                        throw new SqliteException($"Reading.Id not provided after insert. Timestamp:{((DateTime)readingItem.Key.Timestamp).ToString("o")}, LabelId:{readingItem.Key.LabelId}, Reading.Id:{readingId}. Insert rows affected:{readingsAffected}", 28); // 28: SQLITE Warning
                    }

                    // remember the Reading.Id value for the reading, for foreign key composition.
                    readingItem.Key.Id = readingId.Value;
                }


                // then insert the registers
                var dbRegistersAndReadings = GetDbLiveRegisters(dbReadingsMap, obisIds).ToList();
                foreach (var registerAndReading in dbRegistersAndReadings)
                {
                    var register = registerAndReading.DbLiveRegister;
                    var registersAffected = DbContext.Connection.Execute("INSERT OR IGNORE INTO LiveRegister (ReadingId, ObisId, Value, Scale, Unit) VALUES (@ReadingId, @ObisId, @Value, @Scale, @Unit);",
                      register, transaction);
                    if (registersAffected == 0)
                    {
                        ignoredRegisters.Add((register.ReadingId, register.ObisId), register);
                    }
                }

                // then insert the register tags
                var dbRegisterTags = GetDbLiveRegisterTags(dbReadingsMap, obisIds)
                    .Where(x => !ignoredRegisters.ContainsKey((x.ReadingId, x.ObisId)))
                    .Where(x => x.Tags != 0)
                    .ToList();
                DbContext.Connection.Execute("INSERT INTO LiveRegisterTag (ReadingId, ObisId, Tags) VALUES (@ReadingId, @ObisId, @Tags);", dbRegisterTags, transaction);

                // then touch label obis live
                var dbLabelObisLiveAndReadingId = GetDbLabelObisLive(dbReadingsMap, obisIds)
                    .Where(x => !ignoredRegisters.ContainsKey((x.ReadingId, x.LabelObisLive.ObisId)))
                    .ToList();
                DbContext.Connection.Execute("INSERT OR IGNORE INTO LabelObisLive (LabelId, ObisId, LatestTimestamp) VALUES (@LabelId, @ObisId, @LatestTimestamp); UPDATE LabelObisLive SET LatestTimestamp=@LatestTimestamp WHERE LabelId=@LabelId AND ObisId=@ObisId AND LatestTimestamp < @LatestTimestamp;", dbLabelObisLiveAndReadingId.Select(x => x.LabelObisLive), transaction);

                transaction.Commit();
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                var readingDetails = string.Join(", ", dbReadingsMap
                  .Select(x => $"(Label:{labels.FirstOrDefault(z => z.Value == x.Key.LabelId)} ({x.Key.LabelId}), Timestamp:{((DateTime)x.Key.Timestamp).ToString("o")}, Id:{x.Key.Id})"));
                throw DataStoreExceptionFactory.Create(e, $"Insert readings failed. Readings:{readingDetails}");
            }

            if (ignoredRegisters.Count > 0)
            {
                var ignoredRegistersByReadingId = ignoredRegisters.Values.GroupBy(x => x.ReadingId).ToList();
                var ignoredReadingsAndRegisters = ignoredRegistersByReadingId.Join(dbReadingsMap, x => x.Key, x => x.Key.Id, (registers, reading) => new { DbRegisters = registers.ToList(), DbReading = reading.Key, Reading = reading.Value }).ToList();
                var details = new System.Text.StringBuilder();
                foreach (var item in ignoredReadingsAndRegisters)
                {
                    details.AppendLine(string.Empty).Append("Label:").Append(item.Reading.Label).Append(", Timestamp:")
                      .Append(item.Reading.Timestamp.ToString("o")).Append(", ObisCodes:").Append(string.Join(", ", item.DbRegisters.Select(x => obisIds.FirstOrDefault(o => o.Value == x.ObisId).Key)));
                }
                logger.LogInformation($"{ignoredRegisters.Count} register(s) and associated tag(s) were ignored during insert to database due to duplicate constraints.{details.ToString()}");
            }
        }

        private static IEnumerable<(Db.LiveRegister DbLiveRegister, Db.LiveReading DbLiveReading)> GetDbLiveRegisters(IDictionary<Db.LiveReading, Reading> dbLiveReadingsMap, IDictionary<ObisCode, byte> obisIds)
        {
            foreach (var entry in dbLiveReadingsMap)
            {
                foreach (var lr in entry.Value.GetRegisterValues())
                {
                    yield return (new Db.LiveRegister { ReadingId = entry.Key.Id, ObisId = obisIds[lr.ObisCode], Value = lr.Value, Scale = lr.Scale, Unit = (byte)lr.Unit }, entry.Key);
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

        private static IEnumerable<(Db.LabelObisLive LabelObisLive, long ReadingId)> GetDbLabelObisLive(IDictionary<Db.LiveReading, Reading> dbLiveReadingsMap, IDictionary<ObisCode, byte> obisIds)
        {
            foreach (var entry in dbLiveReadingsMap)
            {
                foreach (var lr in entry.Value.GetRegisterValues())
                {
                    yield return (new Db.LabelObisLive { LabelId = entry.Key.LabelId, ObisId = obisIds[lr.ObisCode], LatestTimestamp = entry.Key.Timestamp }, entry.Key.Id);
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

        public IList<ObisCode> GetObisCodes(string label, DateTime cutoff)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (cutoff.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(cutoff), $"Must be UTC. Was:{cutoff.Kind}");

            var sql = @"
WITH distinctObis AS 
(
  SELECT DISTINCT reg.ObisId
  FROM Label lbl
  JOIN LiveReading rea ON rea.LabelId=lbl.Id
  JOIN LiveRegister reg ON rea.Id=reg.ReadingId
  WHERE lbl.LabelName=@Label AND rea.Timestamp >= @Cutoff
)
SELECT o.ObisCode
FROM distinctObis
JOIN Obis o ON o.Id=distinctObis.ObisId;";

            UnixTime cutoffUnixTime = cutoff;
            var obisCodes = DbContext.QueryTransaction<long>(sql, new { Label = label, Cutoff = cutoffUnixTime.ToUnixTimeSeconds() });
            return obisCodes.Select(x => (ObisCode)x).ToList();
        }

    }
}
