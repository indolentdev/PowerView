using Dapper;
using Microsoft.Data.Sqlite;

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

      var resultSet = DbContext.QueryTransaction<RowLocal>(sql);
      var meterEvents = ToMeterEvents(resultSet);

      return meterEvents;
    }

    public WithCount<ICollection<MeterEvent>> GetMeterEvents(int skip = 0, int take = 50)
    {
      IEnumerable<RowLocal> resultSet;
      int totalCount; 

      var sql = @"
      SELECT [Label], [DetectTimestamp], [Flag], [Amplification]
      FROM [MeterEvent]
      ORDER BY [Id] DESC
      LIMIT @Take OFFSET @Skip";
      using (var transaction = DbContext.BeginTransaction())
      {
        try 
        {
          resultSet = DbContext.Connection.Query<RowLocal>(sql, new { Take = GetPageCount(take), Skip = skip }, transaction, buffered: true);
          totalCount = DbContext.Connection.ExecuteScalar<int>("SELECT count(*) from MeterEvent", null, transaction);

          transaction.Commit();
        } 
        catch (SqliteException e) 
        {
          transaction.Rollback();
          throw DataStoreExceptionFactory.Create(e);
        }
      }

      var meterEvents = ToMeterEvents(resultSet);

      return new WithCount<ICollection<MeterEvent>>(totalCount, meterEvents);
    }


    private List<MeterEvent> ToMeterEvents(IEnumerable<RowLocal> resultSet)
    {      
      var meterEvents = new List<MeterEvent>();

      foreach (var meterEvent in resultSet)
      {
        var meterEventAmplification = MeterEventAmplificationSerializer.Deserialize(meterEvent.Amplification);
        meterEvents.Add(new MeterEvent(meterEvent.Label, meterEvent.DetectTimestamp, meterEvent.Flag, meterEventAmplification));
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

    private class RowLocal
    {
      public string Label { get; set; }
      public UnixTime DetectTimestamp { get; set; }
      public bool Flag { get; set; }
      public string Amplification { get; set; }

        }

  }
}
