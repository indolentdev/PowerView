﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PowerView.Model.Repository;
using PowerView.Model.Test.Repository;

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
            return dbContext.Insert<TReading, TRegister, Db.LiveRegisterTag>(reading, registers, null);
        }

        internal static (IList<string> Labels, IList<string> DeviceIds) Insert<TReading, TRegister, TRegisterTag>(this DbContext dbContext, TReading reading, TRegister[] registers, TRegisterTag[] registerTags = null)
  where TReading : class, IDbReading
  where TRegister : class, IDbRegister
  where TRegisterTag : class, IDbRegisterTag
        {
            var labelsAndDeviceIds = dbContext.InsertReadings(reading);
            foreach (var register in registers)
            {
                register.ReadingId = reading.Id;
            }
            dbContext.InsertRegisters(registers);

            if (registerTags != null)
            {
                foreach (var registerTag in registerTags)
                {
                    registerTag.ReadingId = reading.Id;
                }
                dbContext.InsertRegisterTags(registerTags);
            }

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
              .Select(x => (Id: x.Key, LabelName: "Label-" + x.Key, Timestamp: x.First().Timestamp))
              .ToArray();

            dbContext.InsertLabels(labels);

            return labels.Select(x => x.LabelName).ToList();
        }

        public static void InsertLabels(this DbContext dbContext, params (byte Id, string LabelName, UnixTime Timestamp)[] labels)
        {
            dbContext.ExecuteTransaction(@"
              INSERT INTO Label (Id, LabelName, Timestamp) 
              SELECT @Id, @LabelName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Label WHERE Id = @Id);", labels.Select(x => new { x.Id, x.LabelName, x.Timestamp }));
        }

        public static List<(byte Id, string Label)> InsertLabels(this DbContext dbContext, params string[] labels)
        {
            dbContext.ExecuteTransaction(@"
              INSERT INTO Label (LabelName, Timestamp) 
              SELECT @LabelName, @Timestamp WHERE NOT EXISTS (SELECT 1 FROM Label WHERE LabelName = @LabelName);",
                labels.Select(x => new { LabelName = x, Timestamp = DateTime.UtcNow }));

            var allLabels = dbContext.QueryTransaction<(byte Id, string LabelName)>("SELECT Id, LabelName FROM Label;");

            return allLabels.Select(x => (x.Id, Label: x.LabelName)).Where(x => labels.Any(l => l == x.Label)).ToList();
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

        internal static void InsertRegisterTags(this DbContext dbContext, params IDbRegisterTag[] dbRegisterTags)
        {
            if (dbRegisterTags.Length == 0) return;

            var sql = "INSERT INTO {0} (ReadingId,ObisId,Tags) VALUES (@ReadingId,@ObisId,@Tags);";

            var dbRegisterGroups = dbRegisterTags.GroupBy(x => x.GetType().Name);
            foreach (var group in dbRegisterGroups)
            {
                var tableName = group.Key;
                var groupSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
                dbContext.ExecuteTransaction(groupSql, group);
            }
        }

        private static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(DbContext dbContext, IDbRegister[] dbRegisters)
        {
            var obisCodes = dbRegisters.Select(x => x.ObisId).Distinct()
                .Select(x => (x, new ObisCode(Enumerable.Repeat(x, 6).ToArray()))).ToArray();

            return InsertObisCodes(dbContext, obisCodes);
        }

        public static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(this DbContext dbContext, params (byte Id, ObisCode ObisCode)[] obisCodes)
        {
            dbContext.ExecuteTransaction(@"
              INSERT INTO Obis (Id, ObisCode) 
              SELECT @Id, @ObisCode WHERE NOT EXISTS (SELECT 1 FROM Obis WHERE Id = @Id);",
                  obisCodes.Select(x => new { x.Id, ObisCode = (long)x.ObisCode }).ToList());

            var allObisCodes = dbContext.QueryTransaction<(byte Id, long ObisCode)>("SELECT Id, ObisCode FROM Obis;");

            return allObisCodes.Select(x => (x.Id, (ObisCode)x.ObisCode)).Where(x => obisCodes.Any(oc => oc.Id == x.Id)).ToList();
        }

        public static List<(byte Id, ObisCode ObisCode)> InsertObisCodes(this DbContext dbContext, params ObisCode[] obisCodes)
        {
            return InsertObisCodes(dbContext, obisCodes.ToList());
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

        public static void InsertLabelObisLive(this DbContext dbContext, params (byte LabelId, byte ObisId, UnixTime LatestTimestamp)[] lblOcLive)
        {
            dbContext.ExecuteTransaction("INSERT INTO LabelObisLive (LabelId, ObisId, LatestTimestamp) VALUES (@LabelId, @ObisId, @LatestTimestamp);",
              lblOcLive.Select(x => new { x.LabelId, x.ObisId, x.LatestTimestamp }));
        }

        public static void InsertStreamPosition(this DbContext dbContext, params (string StreamName, byte LabelId, long Position)[] streamPositions)
        {
            dbContext.ExecuteTransaction("INSERT INTO StreamPosition (StreamName,LabelId,Position) VALUES (@StreamName,@LabelId,@Position);",
              streamPositions.Select(x => new { x.StreamName, x.LabelId, x.Position }));
        }

        public static void InsertCostBreakdown(this DbContext dbContext, Db.CostBreakdown costBreakdown, params Db.CostBreakdownEntry[] costBreakdownEntries)
        {
            var id = dbContext.QueryTransaction<long>(
              "INSERT INTO CostBreakdown (Title,Currency,Vat) VALUES (@Title,@Currency,@Vat); SELECT last_insert_rowid();",
              costBreakdown).First();
            costBreakdown.Id = id;

            foreach (var entry in costBreakdownEntries)
            {
                entry.CostBreakdownId = id;
            }

            dbContext.ExecuteTransaction(
              "INSERT INTO CostBreakdownEntry (CostBreakdownId,FromDate,ToDate,Name,StartTime,EndTime,Amount) VALUES (@CostBreakdownId,@FromDate,@ToDate,@Name,@StartTime,@EndTime,@Amount);", costBreakdownEntries);
        }

        public static void InsertGeneratorSeries(this DbContext dbContext, params Db.GeneratorSeries[] generatorSeries)
        {
            dbContext.ExecuteTransaction(
              "INSERT INTO GeneratorSeries (Label, ObisCode, BaseLabelId, BaseObisId, CostBreakdownTitle) VALUES (@Label, @ObisCode, @BaseLabelId, @BaseObisId, @CostBreakdownTitle);",
              generatorSeries);
        }

    }
}
