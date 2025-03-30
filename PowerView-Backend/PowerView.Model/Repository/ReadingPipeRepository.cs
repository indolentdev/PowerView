using System.Globalization;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class ReadingPipeRepository : RepositoryBase, IReadingPipeRepository
  {
    private readonly ILocationContext locationContext;
    private readonly int readingsPerLabel;

    public ReadingPipeRepository(IDbContext dbContext, ILocationContext locationContext)
      : this(dbContext, locationContext, 9280) // 9280 ~ roughly 32 days with 5 min intervals.)
    {
    }

    internal ReadingPipeRepository(IDbContext dbContext, ILocationContext locationContext, int readingsPerLabel)
      : base(dbContext)
    {
            ArgumentNullException.ThrowIfNull(locationContext);

            this.locationContext = locationContext;
      this.readingsPerLabel = readingsPerLabel;
    }

    public bool PipeLiveReadingsToDayReadings(DateTime maximumDateTime)
    {
      ArgCheck.ThrowIfNotUtc(maximumDateTime);

      return PipeReadings<Db.LiveReading, Db.DayReading>(maximumDateTime);
    }

    public bool PipeDayReadingsToMonthReadings(DateTime maximumDateTime)
    {
      ArgCheck.ThrowIfNotUtc(maximumDateTime);

      return PipeReadings<Db.DayReading, Db.MonthReading>(maximumDateTime);
    }

    public void PipeMonthReadingsToYearReadings(DateTime maximumDateTime)
    {
      ArgCheck.ThrowIfNotUtc(maximumDateTime);

      PipeReadings<Db.MonthReading, Db.YearReading>(maximumDateTime);
    }

    private bool PipeReadings<TSrcReading, TDstReading>(DateTime maximumDateTime) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      var existingLabelToTimeStamp = GetLabelIdMaxTimestamps<TDstReading>();

      var streamPositions = GetStreamPositions<TDstReading>();
      var readingsByLabelId = GetReadingsByLabelId<TSrcReading>(streamPositions);
      var readingsToPipeByLabel = GetReadingsToPipeByLabelId<TSrcReading, TDstReading>(maximumDateTime, existingLabelToTimeStamp, readingsByLabelId);

      var pipedSomething = PipeReadings<TSrcReading, TDstReading>(readingsToPipeByLabel);

      return pipedSomething;
    }

    private Dictionary<byte, long> GetStreamPositions<TDstReading>()
      where TDstReading : class, IDbReading
    {
      var streamName = GetTableName<TDstReading>();
      var resultSet = DbContext.QueryTransaction<Db.StreamPosition>(
        "SELECT Id,StreamName,LabelId,Position FROM StreamPosition WHERE StreamName = @streamName;",
        new { streamName });
      return resultSet.ToDictionary(sp => sp.LabelId, sp => sp.Position);
    }
      
    private Dictionary<byte, DateTime> GetLabelIdMaxTimestamps<TDstReading>()
      where TDstReading : class, IDbReading
    {
      var tableName = GetTableName<TDstReading>();
      var labelToTimeStamp = new Dictionary<byte, DateTime>(4);

      var sqlQuery = "SELECT LabelId, MAX(Timestamp) AS MaxTimeStamp FROM {0} GROUP BY LabelId;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var resultSet = DbContext.QueryTransaction<RowLocal>(sqlQuery);
      foreach (var row in resultSet)
      {
        labelToTimeStamp.Add(row.LabelId, row.MaxTimestamp);
      }

      return labelToTimeStamp;
    }

    private class RowLocal
    {
      public byte LabelId { get; set; }
      public UnixTime MaxTimestamp { get; set; }
    }

    private Dictionary<byte, IList<TSrcReading>> GetReadingsByLabelId<TSrcReading>(Dictionary<byte, long> streamPositions)
      where TSrcReading : class, IDbReading
    {
      var labelIds = GetLabelIds<TSrcReading>();

      var resultSet = new Dictionary<byte, IList<TSrcReading>>(labelIds.Count);

      foreach (var labelId in labelIds)
      {
        long position = 0;
        if (streamPositions.TryGetValue(labelId, out var labelIdPosition))
        {
          position = labelIdPosition;
        }
        
        var readings = GetReadings<TSrcReading>(labelId, position, readingsPerLabel);
        resultSet.Add(labelId, readings);
      }

      return resultSet;
    }

    private List<byte> GetLabelIds<TSrcReading>()
      where TSrcReading : class, IDbReading
    {
      var labels = new List<byte>(5);
      
      var tableName = GetTableName<TSrcReading>();

      UnixTime timestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(600)); // cap.. max almost two years..

      var sqlQuery = "SELECT DISTINCT LabelId FROM {0} WHERE Timestamp > @Timestamp;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var resultSet = DbContext.QueryTransaction<byte>(sqlQuery, new { Timestamp = timestamp });
      foreach (var row in resultSet)
      {
        labels.Add(row);
      }
      return labels;
    }

    private IList<TSrcReading> GetReadings<TSrcReading>(byte labelId, long position, int limit)
      where TSrcReading : class, IDbReading
    {
      var tableName = GetTableName<TSrcReading>();

      var sqlQuery = "SELECT * FROM {0} WHERE Id > @Position AND LabelId = @LabelId ORDER BY Id ASC LIMIT @Limit;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var args = new { LabelId = labelId, Position = position, Limit = limit };
      var resultSet = DbContext.QueryTransaction<TSrcReading>(sqlQuery, args);

      return resultSet;
    }

    private Dictionary<byte, IEnumerable<TSrcReading>> GetReadingsToPipeByLabelId<TSrcReading, TDstReading>(DateTime maximumDateTime, Dictionary<byte, DateTime> existingLabelIdToTimeStamp, IDictionary<byte, IList<TSrcReading>> readingsByLabelId)
      where TSrcReading : class, IDbReading
      where TDstReading : class, IDbReading
    {
      var readingsToPipe = new Dictionary<byte, IEnumerable<TSrcReading>>(readingsByLabelId.Count);
      foreach (var labelGrouping in readingsByLabelId)
      {
        var labelId = labelGrouping.Key;
        var minimumDateTime = DateTime.MinValue.ToUniversalTime();
        if (existingLabelIdToTimeStamp.TryGetValue(labelId, out var labelIdDateTime))
        {
          minimumDateTime = labelIdDateTime;
        }
        var reducedLabelReadings = Reduce<TSrcReading, TDstReading>(labelGrouping.Value, minimumDateTime, maximumDateTime);
        readingsToPipe.Add(labelId, reducedLabelReadings);
      }

      return readingsToPipe;
    }

    private IEnumerable<TSrcReading> Reduce<TSrcReading, TDstReading>(IEnumerable<TSrcReading> readings, DateTime minimumDateTime, DateTime maximumDateTime) 
      where TSrcReading : class, IDbReading
      where TDstReading : class, IDbReading
    {
      var orderedReadings = readings.OrderBy(r => r.Timestamp)
        .Select(r => new ReduceItem<TSrcReading>(r, ChangeTimeZoneAndReduce<TDstReading>(r.Timestamp))).ToList();

      var coarseMinimumDateTime = ChangeTimeZoneAndReduce<TDstReading>(minimumDateTime);
      var coarseMaximumDateTime = ChangeTimeZoneAndReduce<TDstReading>(maximumDateTime);

      var result = new List<ReduceItem<TSrcReading>>(32);

      // Group readings by coarse time and pick the last reading within each coarse time.
      var labelReadingsByTimeGroup = orderedReadings.GroupBy(x => x.CoarseTimestamp);
      foreach (var readingGrouping in labelReadingsByTimeGroup)
      {
        var orderedReadingsInTimeGroup = readingGrouping.OrderByDescending(x => x.Reading.Timestamp);
        result.Add(orderedReadingsInTimeGroup.First());
      }

      // Inspect the most recent coarse timestamp grouping as it may be incomplete
      // I.e. not be a complete day/month - in case readingsPerLabel "cut the day/month readings in half".
      var mostRecentCoarseTimestampGroup = labelReadingsByTimeGroup.OrderByDescending(x => x.Key).FirstOrDefault();
      if (mostRecentCoarseTimestampGroup != null)
      {
        var mostRecentReading = mostRecentCoarseTimestampGroup.OrderByDescending(x => x.Reading.Timestamp).FirstOrDefault();
        if (mostRecentReading != null && !IsTimestampSatisfactory<TDstReading>(mostRecentReading.Reading.Timestamp))
        {
          coarseMaximumDateTime = mostRecentCoarseTimestampGroup.Key; // check for a "complete time".
        }
      }

      // Also search the readings.. if a meter exchange has happened then include both readings.
      for (var ix = 0; ix < orderedReadings.Count - 1; ix++)
      { 
        var a = orderedReadings[ix];
        var b = orderedReadings[ix+1];
        if (DeviceId.Equals(a.Reading.DeviceId, b.Reading.DeviceId))
        {
          continue;
        }
        result.Add(a);
        result.Add(b);
      }

      return result.Distinct().OrderBy(x => x.Reading.Timestamp)
        .Where(x => x.CoarseTimestamp > coarseMinimumDateTime && x.CoarseTimestamp < coarseMaximumDateTime)
        .Select(x => x.Reading);
    }

    private class ReduceItem<TSrcReading>
      where TSrcReading : class, IDbReading
    {
      public ReduceItem(TSrcReading reading, DateTime coarseTimestamp)
      {
        Reading = reading;
        CoarseTimestamp = coarseTimestamp;
      }

      public TSrcReading Reading { get; private set; }
      public DateTime CoarseTimestamp { get; private set; }
    }

    private DateTime ChangeTimeZoneAndReduce<TDstReading>(DateTime dateTime)
      where TDstReading : class, IDbReading
    {
      var zonedDateTime = locationContext.ConvertTimeFromUtc(dateTime);
      return ReadingPipeRepositoryHelper.Reduce<TDstReading>(zonedDateTime);
    }

    private bool IsTimestampSatisfactory<TDstReading>(DateTime dateTime)
      where TDstReading : class, IDbReading
    {
      var fraction = GetFraction<TDstReading>();
      var zonedDateTime = locationContext.ConvertTimeFromUtc(dateTime);
      return ReadingPipeRepositoryHelper.IsGreaterThanResolutionFraction<TDstReading>(fraction, zonedDateTime);
    }

    private static double GetFraction<TDstReading>()
      where TDstReading : class, IDbReading
    {
      var typeName = typeof(TDstReading).Name;
      switch (typeName)
      {
        case "DayReading":
          return 0.625; // Represents 15:00 within a 24 hour clock.
        case "MonthReading":
          return 0.98; // 27.44 days for 28 day month and 30.38 for 31 day month.
        case "YearReading":
          return 0.83; // 9.96 months
        default:
          throw new NotSupportedException("Resolution not supported: " + typeName);
      }
    }

    private bool PipeReadings<TSrcReading, TDstReading>(IDictionary<byte, IEnumerable<TSrcReading>> readingsToPipeByLabel) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      var result = false;
      IList<IDbRegister> registers;
      foreach (var labelGrouping in readingsToPipeByLabel)
      {
        var labelId = labelGrouping.Key;
        var readingsToPipe = labelGrouping.Value;

        if (typeof(TSrcReading) == typeof(Db.LiveReading))
        {
          registers = GetRegisters<TSrcReading, Db.LiveRegister>(labelId, readingsToPipe);
        }
        else if (typeof(TSrcReading) == typeof(Db.DayReading))
        {
          registers = GetRegisters<TSrcReading, Db.DayRegister>(labelId, readingsToPipe);
        }
        else if (typeof(TSrcReading) == typeof(Db.MonthReading))
        {
          registers = GetRegisters<TSrcReading, Db.MonthRegister>(labelId, readingsToPipe);
        }
        else
        {
          throw new NotSupportedException(typeof(TSrcReading) + " not supported. Extend this method");
        }

        foreach (var reading in readingsToPipe.OrderBy(r => r.Id))
        {
          var registersForReading = registers.Where(r => r.ReadingId == reading.Id).ToList();

          InsertReadingAndRegisters<TSrcReading, TDstReading>(labelId, reading, registersForReading);

          result = true;
        }
      }
      return result;
    }

    private List<IDbRegister> GetRegisters<TSrcReading, TSrcRegister>(byte labelId, IEnumerable<TSrcReading> readingsToPipe)
      where TSrcReading : class, IDbReading
      where TSrcRegister : class, IDbRegister
    {
      var tableName = GetTableName<TSrcRegister>();

      var sqlQuery = "SELECT * FROM {0} WHERE ReadingId IN ({1});";
      var ids = string.Join(", ", readingsToPipe.Select(r => r.Id.ToString(CultureInfo.InvariantCulture)));
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName, ids);
      var resultSet = DbContext.QueryTransaction<TSrcRegister>(sqlQuery);

      return resultSet.Cast<IDbRegister>().ToList();
    }

    private void InsertReadingAndRegisters<TSrcReading, TDstReading>(byte labelId, TSrcReading reading, ICollection<IDbRegister> registers) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      var dstReadingTable = GetTableName<TDstReading>();
      var dstRegisterTable = dstReadingTable.Replace("Reading", "Register");

      using var transaction = DbContext.BeginTransaction();
      try
      {
        var sql = "INSERT INTO {0} (LabelId, DeviceId, Timestamp) VALUES (@LabelId, @DeviceId, @Timestamp); SELECT last_insert_rowid();";
        sql = string.Format(CultureInfo.InvariantCulture, sql, dstReadingTable);
        var dstReading = ToDstReading<TDstReading>(reading);
        dstReading.Id = DbContext.Connection.QueryFirst<long>(sql, dstReading, transaction);

        foreach (var register in registers)
        {
          register.ReadingId = dstReading.Id;
        }
        sql = "INSERT INTO {0} (ReadingId,ObisId,Value,Scale,Unit) VALUES (@ReadingId,@ObisId,@Value,@Scale,@Unit);";
        sql = string.Format(CultureInfo.InvariantCulture, sql, dstRegisterTable);
        DbContext.Connection.Execute(sql, registers, transaction);

        // Upsert StreamPosition
        sql = "UPDATE StreamPosition SET Position=@Position WHERE StreamName=@StreamName AND LabelId=@LabelId;";
        var affectedRecords = DbContext.Connection.Execute(sql, new { Position = reading.Id, StreamName = dstReadingTable, LabelId = labelId }, transaction);
        if (affectedRecords == 0)
        {
          var streamPosition = new Db.StreamPosition { StreamName = dstReadingTable, LabelId = labelId, Position = reading.Id };
          sql = "INSERT INTO StreamPosition (StreamName,LabelId,Position) VALUES (@StreamName, @LabelId, @Position);";
          DbContext.Connection.Execute(sql, streamPosition, transaction);
        }

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    private static TDstReading ToDstReading<TDstReading>(IDbReading srcReading) 
      where TDstReading : class, IDbReading, new()
    {
      var dstReading = new TDstReading
      { 
        LabelId = srcReading.LabelId,
        DeviceId = srcReading.DeviceId,
        Timestamp = srcReading.Timestamp
      };
      return dstReading;
    }

    private static string GetTableName<T>()
    {
      return typeof(T).Name;
    }
  }
}

