using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    internal static class DbContextExtensions
    {
        internal static int GetCount<TEntity>(this DbContext dbContext)
        {
            var table = typeof(TEntity).Name;
            var sql = "SELECT count(*) FROM {0};";
            sql = string.Format(CultureInfo.InvariantCulture, sql, table);
            return dbContext.QueryTransaction<int>(sql).First();
        }

        internal static (IList<string> Labels, IList<string> DeviceIds) Insert<TReading, TRegister>(this DbContext dbContext, TReading reading, params TRegister[] registers)
  where TReading : class, IDbReading
  where TRegister : class, IDbRegister
        {
            var labelsAndDeviceIds = dbContext.InsertReadings(reading);
            foreach (var register in registers)
            {
                register.ReadingId = reading.Id;
            }
            dbContext.InsertRegisters(registers);
            return labelsAndDeviceIds;
        }

        internal static (IList<string> Labels, IList<string> DeviceIds) InsertReadings(this DbContext dbContext, params IDbReading[] dbReadings)
        {
            if (dbReadings.Length == 0) return (Array.Empty<string>(), Array.Empty<string>());

            var labels = InsertLabels(dbContext, dbReadings);
            var deviceIds = InsertDevices(dbContext, dbReadings);

            var sql = "INSERT INTO {0} (LabelId,DeviceId,Timestamp) VALUES (@LabelId,@DeviceId,@Timestamp); SELECT last_insert_rowid();";
            foreach (var dbReading in dbReadings)
            {
                var tableName = dbReading.GetType().Name;
                var readingSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
                var id = dbContext.QueryTransaction<long>(readingSql, dbReading).First();
                dbReading.Id = id;
            }

            return (labels, deviceIds);
        }

        private static List<string> InsertLabels(DbContext dbContext, IDbReading[] dbReadings)
        {
            var now = DateTime.UtcNow;
            var labels = dbReadings.Select(x => x.LabelId).Distinct().Select(x => new { Id = x, LabelName = "Label-" + x, Timestamp = now }).ToList();

            dbContext.ExecuteTransaction(@"
              INSERT INTO Label (Id, LabelName, Timestamp) 
              SELECT @Id, @LabelName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Label WHERE Id = @Id);", labels);

            return labels.Select(x => x.LabelName).ToList();
        }

        private static List<string> InsertDevices(DbContext dbContext, IDbReading[] dbReadings)
        {
            var now = DateTime.UtcNow;
            var devices = dbReadings.Select(x => x.DeviceId).Distinct().Select(x => new { Id = x, DeviceName = "DeviceId-" + x, Timestamp = now }).ToList();

            dbContext.ExecuteTransaction(@"
              INSERT INTO Device (Id, DeviceName, Timestamp) 
              SELECT @Id, @DeviceName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Device WHERE Id = @Id);", devices);

            return devices.Select(x => x.DeviceName).ToList();
        }

        internal static void InsertRegisters(this DbContext dbContext, params IDbRegister[] dbRegisters)
        {
            if (dbRegisters.Length == 0) return;

            var sql = "INSERT INTO {0} (ObisCode,Value,Scale,Unit,ReadingId) VALUES (@ObisCode,@Value,@Scale,@Unit,@ReadingId);";

            var dbRegisterGroups = dbRegisters.GroupBy(x => x.GetType().Name);
            foreach (var group in dbRegisterGroups)
            {
                var tableName = group.Key;
                var groupSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
                dbContext.ExecuteTransaction(groupSql, group);
            }
        }

    }
}
