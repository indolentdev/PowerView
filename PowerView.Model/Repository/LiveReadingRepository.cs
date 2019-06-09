using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using DapperExtensions;

namespace PowerView.Model.Repository
{
  internal class LiveReadingRepository : RepositoryBase, ILiveReadingRepository
  {
    //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

      var dbLiveReadingsMap = liveReadings.ToDictionary(lr => new Db.LiveReading { Label = lr.Label, SerialNumber = lr.SerialNumber, Timestamp = lr.Timestamp });
      var transaction = DbContext.BeginTransaction();
      try
      {
        foreach (var liveReading in dbLiveReadingsMap.Keys)
        {
          DbContext.Connection.Insert(liveReading, transaction);
        }
        var dbLiveRegisters = GetDbLiveRegisters(dbLiveReadingsMap);
        DbContext.Connection.Insert<Db.LiveRegister>(dbLiveRegisters, transaction);
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
          yield return new Db.LiveRegister { ObisCode = lr.ObisCode, Value = lr.Value, Scale = lr.Scale, Unit = (byte)lr.Unit, ReadingId = entry.Key.Id };
        }
      }
    }

  }
}
