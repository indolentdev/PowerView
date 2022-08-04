﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using System.Data.SQLite;

namespace PowerView.Model.Repository
{
  internal class MeterEventRepository : RepositoryBase, IMeterEventRepository
  {
    public MeterEventRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public ICollection<MeterEvent> GetLatestMeterEventsByLabel()
    {
      var sql = @"
      SELECT [Label], Max([DetectTimestamp]) AS DetectTimestamp, [Flag], [Amplification]
      FROM [MeterEvent]
      GROUP BY [Label], [MeterEventType]
      ORDER BY [DetectTimestamp]";

      var resultSet = DbContext.QueryTransaction<dynamic>(sql);
      var meterEvents = ToMeterEvents(resultSet);

      return meterEvents;
    }

    public WithCount<ICollection<MeterEvent>> GetMeterEvents(int skip = 0, int take = 50)
    {
      IEnumerable<dynamic> resultSet;
      int totalCount; 

      var sql = @"
      SELECT [Label], [DetectTimestamp], [Flag], [Amplification]
      FROM [MeterEvent]
      ORDER BY [Id] DESC
      LIMIT @Take OFFSET @Skip";
      var transaction = DbContext.BeginTransaction();
      try 
      {
        resultSet = DbContext.Connection.Query(sql, new { Take = GetPageCount(take), Skip = skip }, transaction, buffered: true);
        totalCount = DbContext.Connection.ExecuteScalar<int>("SELECT count(*) from MeterEvent", null, transaction);

        transaction.Commit();
      } 
      catch (SQLiteException e) 
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      var meterEvents = ToMeterEvents(resultSet);

      return new WithCount<ICollection<MeterEvent>>(totalCount, meterEvents);
    }


    private List<MeterEvent> ToMeterEvents(IEnumerable<dynamic> resultSet)
    {      
      var meterEvents = new List<MeterEvent>();

      Type dateTimeType = typeof(DateTime);
      Type longType = typeof(long);
      Type timestampType = null;
      foreach (dynamic meterEvent in resultSet)
      {
        if (timestampType == null) 
        {
          timestampType = meterEvent.DetectTimestamp.GetType();
        }

        string label = meterEvent.Label;
        DateTime detectTimestamp;
        if (timestampType == longType)
        {
          long maxDetectTimestamp = meterEvent.DetectTimestamp;
          detectTimestamp = DbContext.GetDateTime(maxDetectTimestamp);
        }
        else if (timestampType == dateTimeType)
        {
          detectTimestamp = meterEvent.DetectTimestamp;
        }
        else
        {
          throw new DataStoreException("Unable to map timestamp type. Type:" + timestampType.Name);
        }

        long flagLong = meterEvent.Flag;
        string amplification = meterEvent.Amplification;
        var meterEventAmplification = MeterEventAmplificationSerializer.Deserialize(amplification);
        var flag = flagLong > 0 ? true : false;
        meterEvents.Add(new MeterEvent(label, detectTimestamp, flag, meterEventAmplification));
      }

      return meterEvents;
    }

    public void AddMeterEvents(IEnumerable<MeterEvent> newMeterEvents)
    {
      if (newMeterEvents == null) throw new ArgumentNullException("newMeterEvents");

      var dbEntities = newMeterEvents.OrderBy(me => me.DetectTimestamp).Select(ToDbEntity);
      DbContext.ExecuteTransaction(
        "INSERT INTO MeterEvent (Label,MeterEventType,DetectTimestamp,Flag,Amplification) VALUES (@Label,@MeterEventType,@DetectTimestamp,@Flag,@Amplification);", 
        dbEntities);
    }

    private static Db.MeterEvent ToDbEntity(MeterEvent meterEvent)
    {
      var amplification = MeterEventAmplificationSerializer.Serialize(meterEvent.Amplification);
      return new Db.MeterEvent { Label=meterEvent.Label, MeterEventType=meterEvent.Amplification.GetMeterEventType(), 
        DetectTimestamp=meterEvent.DetectTimestamp, Flag=meterEvent.Flag, Amplification=amplification };
    }

    public long? GetMaxFlaggedMeterEventId()
    {
      const string sql = @"
      SELECT Max([Id])
      FROM [MeterEvent]
      WHERE [Flag]=@flag";
      return DbContext.QueryTransaction<long?>(sql, new { flag = true }).First();
    }

  }
}
