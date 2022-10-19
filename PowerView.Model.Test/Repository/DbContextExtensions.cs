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
            var labels = dbReadings
              .GroupBy(x => x.LabelId)
              .Select(x => new { Id = x.Key, LabelName = "Label-" + x.Key, Timestamp = (UnixTime)x.First().Timestamp })
              .ToList();

            dbContext.ExecuteTransaction(@"
              INSERT INTO Label (Id, LabelName, Timestamp) 
              SELECT @Id, @LabelName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Label WHERE Id = @Id);", labels);

            return labels.Select(x => x.LabelName).ToList();
        }

        private static List<string> InsertDevices(DbContext dbContext, IDbReading[] dbReadings)
        {
            UnixTime now = DateTime.UtcNow;
            var devices = dbReadings.Select(x => x.DeviceId).Distinct().Select(x => new { Id = x, DeviceName = "DeviceId-" + x, Timestamp = now }).ToList();

            dbContext.ExecuteTransaction(@"
              INSERT INTO Device (Id, DeviceName, Timestamp) 
              SELECT @Id, @DeviceName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Device WHERE Id = @Id);", devices);

            return devices.Select(x => x.DeviceName).ToList();
        }

        internal static IList<ObisCode> InsertRegisters(this DbContext dbContext, params IDbRegister[] dbRegisters)
        {
            if (dbRegisters.Length == 0) return Array.Empty<ObisCode>();

            var obisCodes = InsertObisCodes(dbContext, dbRegisters);

            var sql = "INSERT INTO {0} (ReadingId,ObisId,Value,Scale,Unit) VALUES (@ReadingId,@ObisId,@Value,@Scale,@Unit);";

            var dbRegisterGroups = dbRegisters.GroupBy(x => x.GetType().Name);
            foreach (var group in dbRegisterGroups)
            {
                var tableName = group.Key;
                var groupSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
                dbContext.ExecuteTransaction(groupSql, group);
            }

            return obisCodes.Select(x => x.ObisCode).ToList();
        }

        private static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(DbContext dbContext, IDbRegister[] dbRegisters)
        {
            var obisCodes = dbRegisters.Select(x => x.ObisId).Distinct()
                .Select(x => ( x, new ObisCode(Enumerable.Repeat(x, 6).ToArray()) )).ToArray();

            return InsertObisCodes(dbContext, obisCodes);
        }

        public static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(this DbContext dbContext, params (byte Id, ObisCode ObisCode)[] obisCodes)
        {
            dbContext.ExecuteTransaction(@"
              INSERT INTO Obis (Id, ObisCode) 
              SELECT @Id, @ObisCode WHERE NOT EXISTS (SELECT 1 FROM Obis WHERE Id = @Id);", 
                  obisCodes.Select(x => new { x.Id, ObisCode = (long)x.ObisCode}).ToList());

            var allObisCodes = dbContext.QueryTransaction<(byte Id, long ObisCode)>("SELECT Id, ObisCode FROM Obis;");

            return allObisCodes.Select(x => (x.Id, (ObisCode)x.ObisCode)).Where(x => obisCodes.Any(oc => oc.Id == x.Id)).ToList();
        }

        public static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(this DbContext dbContext, IList<ObisCode> obisCodes)
        {
            dbContext.ExecuteTransaction(@"
              INSERT INTO Obis (ObisCode) 
              SELECT @ObisCode WHERE NOT EXISTS (SELECT 1 FROM Obis WHERE ObisCode = @ObisCode);",
                  obisCodes.Select(x => new { ObisCode = (long)x }).ToList());

            var allObisCodes = dbContext.QueryTransaction<(byte Id, long ObisCode)>("SELECT Id, ObisCode FROM Obis;");

            return allObisCodes.Select(x => (x.Id, ObisCode: (ObisCode)x.ObisCode)).Where(x => obisCodes.Any(oc => oc == x.ObisCode)).ToList();
        }

    }
}
