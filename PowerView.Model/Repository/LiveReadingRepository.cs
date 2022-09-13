using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add(IList<LiveReading> liveReadings)
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

            var dbLiveReadingsMap = liveReadings.ToDictionary(lr => new Db.LiveReading { LabelId = labels[lr.Label], DeviceId = deviceIds[lr.DeviceId], Timestamp = lr.Timestamp });
            using var transaction = DbContext.BeginTransaction();
            try
            {
                // First insert the readings
                foreach (var liveReading in dbLiveReadingsMap.Keys)
                {
                    liveReading.Id = DbContext.Connection.QueryFirst<long>(
                      "INSERT INTO LiveReading (LabelId, DeviceId, Timestamp) VALUES (@LabelId, @DeviceId, @Timestamp); SELECT last_insert_rowid();",
                      liveReading, transaction);
                }
                // then insert the registers
                var dbLiveRegisters = GetDbLiveRegisters(dbLiveReadingsMap);
                DbContext.Connection.Execute(
                  "INSERT INTO LiveRegister (ReadingId, ObisCode, Value, Scale, Unit) VALUES (@ReadingId, @ObisCode, @Value, @Scale, @Unit);",
                  dbLiveRegisters, transaction);
                transaction.Commit();
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                throw DataStoreExceptionFactory.Create(e);
            }
        }

        private static IEnumerable<Db.LiveRegister> GetDbLiveRegisters(IDictionary<Db.LiveReading, LiveReading> dbLiveReadingsMap)
        {
            foreach (var entry in dbLiveReadingsMap)
            {
                foreach (var lr in entry.Value.GetRegisterValues())
                {
                    yield return new Db.LiveRegister { ReadingId = entry.Key.Id, ObisCode = lr.ObisCode, Value = lr.Value, Scale = lr.Scale, Unit = (byte)lr.Unit };
                }
            }
        }

        private IList<(byte Id, string Label)> GetLabelIds(List<string> labels)
        {
            var now = DateTime.UtcNow;
            var labelsAndTimestamps = labels.Select(x => new { LabelName = x, Timestamp = now });
            DbContext.ExecuteTransaction(@"
              INSERT INTO Label (LabelName, Timestamp) VALUES (@LabelName, @Timestamp)
                ON CONFLICT(LabelName) DO UPDATE SET Timestamp = @Timestamp;", labelsAndTimestamps);

            return DbContext.QueryTransaction<(byte Id, string Label)>("SELECT Id, LabelName FROM Label;");
        }

        private IList<(byte Id, string DeviceId)> GetDeviceIds(List<string> deviceIds)
        {
            var now = DateTime.UtcNow;
            var deviceIdsAndTimestamps = deviceIds.Select(x => new { DeviceName = x, Timestamp = now });
            DbContext.ExecuteTransaction(@"
              INSERT INTO Device (DeviceName, Timestamp) VALUES (@DeviceName, @Timestamp)
                ON CONFLICT(DeviceName) DO UPDATE SET Timestamp = @Timestamp;", deviceIdsAndTimestamps);

            return DbContext.QueryTransaction<(byte Id, string DeviceId)>("SELECT Id, DeviceName FROM Device;");
        }

    }
}
