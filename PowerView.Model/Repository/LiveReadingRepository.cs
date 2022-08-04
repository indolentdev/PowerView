using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
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

      var dbLiveReadingsMap = liveReadings.ToDictionary(lr => new Db.LiveReading { Label = lr.Label, DeviceId = lr.DeviceId, Timestamp = lr.Timestamp });
      var transaction = DbContext.BeginTransaction();
      try
      {
        // First insert the readings
        foreach (var liveReading in dbLiveReadingsMap.Keys)
        {
          liveReading.Id = DbContext.Connection.QueryFirst<long>(
            "INSERT INTO LiveReading (Label, DeviceId, Timestamp) VALUES (@Label, @DeviceId, @Timestamp); SELECT last_insert_rowid();",
            liveReading, transaction);
        }
        // then insert the registers
        var dbLiveRegisters = GetDbLiveRegisters(dbLiveReadingsMap);
        DbContext.Connection.Execute(
          "INSERT INTO LiveRegister (ReadingId, ObisCode, Value, Scale, Unit) VALUES (@ReadingId, @ObisCode, @Value, @Scale, @Unit);",
          dbLiveRegisters, transaction);
        transaction.Commit();
      }
      catch (SQLiteException e)
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

  }
}
