using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using Dapper;
using log4net;

namespace PowerView.Model.Repository
{
  internal class ReadingPipeRepository : RepositoryBase, IReadingPipeRepository
  {
    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ILocationContext locationContext;
    private readonly int readingsPerLabel;

    public ReadingPipeRepository(IDbContext dbContext, ILocationContext locationContext)
      : this(dbContext, locationContext, 9280) // 9280 ~ roughly 32 days with 5 min intervals.)
    {
    }

    internal ReadingPipeRepository(IDbContext dbContext, ILocationContext locationContext, int readingsPerLabel)
      : base(dbContext)
    {
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.locationContext = locationContext;
      this.readingsPerLabel = readingsPerLabel;
    }

    public bool PipeLiveReadingsToDayReadings(DateTime maximumDateTime)
    {
      if (maximumDateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("maximumDateTime", "Must be UTC time");

      return PipeReadings<Db.LiveReading, Db.DayReading>(maximumDateTime);
    }

    public bool PipeDayReadingsToMonthReadings(DateTime maximumDateTime)
    {
      if (maximumDateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("maximumDateTime", "Must be UTC time");

      return PipeReadings<Db.DayReading, Db.MonthReading>(maximumDateTime);
    }

    public void PipeMonthReadingsToYearReadings(DateTime maximumDateTime)
    {
      if (maximumDateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("maximumDateTime", "Must be UTC time");

      PipeReadings<Db.MonthReading, Db.YearReading>(maximumDateTime);
    }

    private bool PipeReadings<TSrcReading, TDstReading>(DateTime maximumDateTime) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      log.DebugFormat("Starting piping {0} to {1}", typeof(TSrcReading).Name, typeof(TDstReading).Name);

      var existingLabelToTimeStamp = GetLabelMaxTimestamps<TDstReading>();

      var streamPositions = GetStreamPositions<TDstReading>();
      var readingsByLabel = GetReadingsByLabel<TSrcReading>(streamPositions);
      var readingsToPipeByLabel = GetReadingsToPipeByLabel<TSrcReading, TDstReading>(maximumDateTime, existingLabelToTimeStamp, readingsByLabel);

      var pipedSomething = PipeReadings<TSrcReading, TDstReading>(readingsToPipeByLabel);

      log.DebugFormat("Finished piping");

      return pipedSomething;
    }

    private IDictionary<string, long> GetStreamPositions<TDstReading>()
      where TDstReading : class, IDbReading
    {
      var streamName = GetTableName<TDstReading>();
      var resultSet = DbContext.QueryTransaction<Db.StreamPosition>("GetStreamPositions",
        "SELECT Id,StreamName,Label,Position FROM StreamPosition WHERE StreamName = @streamName;",
        new { streamName });
      return resultSet.ToDictionary(sp => sp.Label, sp => sp.Position);
    }
      
    private IDictionary<string, DateTime> GetLabelMaxTimestamps<TDstReading>()
      where TDstReading : class, IDbReading
    {
      var tableName = GetTableName<TDstReading>();
      var labelToTimeStamp = new Dictionary<string, DateTime>(4);

      log.DebugFormat("Querying for labels from {0}", tableName);

      var sqlQuery = "SELECT Label, MAX(Timestamp) AS MaxTimeStampUnix FROM {0} GROUP BY Label;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var resultSet = DbContext.QueryTransaction<dynamic>("GetLabelMaxTimestamps", sqlQuery);
      foreach (dynamic row in resultSet)
      {
        string label = row.Label;
        long maxTimeStampUnix = row.MaxTimeStampUnix;
        labelToTimeStamp.Add(label, DbContext.GetDateTime(maxTimeStampUnix));
      }

      log.DebugFormat("Finished query for lables. Got {0}", labelToTimeStamp.Count);
      return labelToTimeStamp;
    }

    private IDictionary<string, IList<TSrcReading>> GetReadingsByLabel<TSrcReading>(IDictionary<string, long> streamPositions)
      where TSrcReading : class, IDbReading
    {
      var labels = GetLabels<TSrcReading>();

      var resultSet = new Dictionary<string, IList<TSrcReading>>(labels.Count);

      foreach (var label in labels)
      {
        long position = 0;
        if (streamPositions.ContainsKey(label))
        {
          position = streamPositions[label];
        }
        
        var readings = GetReadings<TSrcReading>(label, position, readingsPerLabel);
        resultSet.Add(label, readings);
      }

      return resultSet;
    }

    private ICollection<string> GetLabels<TSrcReading>()
      where TSrcReading : class, IDbReading
    {
      var labels = new List<string>(5);
      
      var tableName = GetTableName<TSrcReading>();

      var timestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(600)); // cap.. max almost two years..

      log.DebugFormat("Querying for labels from {0}", tableName);

      var sqlQuery = "SELECT DISTINCT Label FROM {0} WHERE Timestamp > @Timestamp;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var resultSet = DbContext.QueryTransaction<dynamic>("GetLabels", sqlQuery, new { Timestamp = timestamp });
      foreach (dynamic row in resultSet)
      {
        string label = row.Label;
        labels.Add(label);
      }
      log.DebugFormat("Finished query for labels. Got {0}", labels.Count);

      return labels;
    }

    private IList<TSrcReading> GetReadings<TSrcReading>(string label, long position, int limit)
      where TSrcReading : class, IDbReading
    {
      var tableName = GetTableName<TSrcReading>();

      log.DebugFormat("Querying for {0} readings from position {1}", label, position);

      var sqlQuery = "SELECT * FROM {0} WHERE Id > @Position AND Label = @Label ORDER BY Id ASC LIMIT @Limit;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName);
      var args = new { Label = label, Position = position, Limit = limit };
      var resultSet = DbContext.QueryTransaction<TSrcReading>("GetReadings", sqlQuery, args);
      log.Debug("Finished query for readings");
      return resultSet;
    }

    private IDictionary<string, IEnumerable<TSrcReading>> GetReadingsToPipeByLabel<TSrcReading, TDstReading>(DateTime maximumDateTime, IDictionary<string, DateTime> existingLabelToTimeStamp, IDictionary<string, IList<TSrcReading>> readingsByLabel)
      where TSrcReading : class, IDbReading
      where TDstReading : class, IDbReading
    {
      var readingsToPipe = new Dictionary<string, IEnumerable<TSrcReading>>(readingsByLabel.Count);
      foreach (var labelGrouping in readingsByLabel)
      {
        var label = labelGrouping.Key;
        var minimumDateTime = DateTime.MinValue.ToUniversalTime();
        if (existingLabelToTimeStamp.ContainsKey(label))
        {
          minimumDateTime = existingLabelToTimeStamp[label];
        }
        var reducedLabelReadings = Reduce<TSrcReading, TDstReading>(labelGrouping.Value, minimumDateTime, maximumDateTime);
        if (log.IsDebugEnabled)
        {
          var reducedLabelReadingCount = reducedLabelReadings.Count();
          log.DebugFormat("Reduced {0} {1} reading(s) to {2}", labelGrouping.Value.Count, label, reducedLabelReadingCount);
        }
        readingsToPipe.Add(label, reducedLabelReadings);
      }

      return readingsToPipe;
    }

    private IEnumerable<TSrcReading> Reduce<TSrcReading, TDstReading>(IEnumerable<TSrcReading> readings, DateTime minimumDateTime, DateTime maximumDateTime) 
      where TSrcReading : class, IDbReading
      where TDstReading : class, IDbReading
    {
      var orderedReadings = readings.OrderBy(r => r.Timestamp)
        .Select(r => new ReduceItem<TSrcReading> { Reading=r, CoarseTimestamp=ChangeTimeZoneAndReduce<TDstReading>(r.Timestamp)}).ToArray();

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
      for (var ix = 0; ix < orderedReadings.Length - 1; ix++)
      { 
        var a = orderedReadings[ix];
        var b = orderedReadings[ix+1];
        if (a.Reading.SerialNumber == b.Reading.SerialNumber)
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
      public TSrcReading Reading { get; set; }
      public DateTime CoarseTimestamp { get; set; }
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

    private bool PipeReadings<TSrcReading, TDstReading>(IDictionary<string, IEnumerable<TSrcReading>> readingsToPipeByLabel) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      var result = false;
      IList<IDbRegister> registers;
      foreach (var labelGrouping in readingsToPipeByLabel)
      {
        var label = labelGrouping.Key;
        var readingsToPipe = labelGrouping.Value;

        if (typeof(TSrcReading) == typeof(Db.LiveReading))
        {
          registers = GetRegisters<TSrcReading, Db.LiveRegister>(label, readingsToPipe);
        }
        else if (typeof(TSrcReading) == typeof(Db.DayReading))
        {
          registers = GetRegisters<TSrcReading, Db.DayRegister>(label, readingsToPipe);
        }
        else if (typeof(TSrcReading) == typeof(Db.MonthReading))
        {
          registers = GetRegisters<TSrcReading, Db.MonthRegister>(label, readingsToPipe);
        }
        else
        {
          throw new NotSupportedException(typeof(TSrcReading) + " not supported. Extend this method");
        }

        foreach (var reading in readingsToPipe.OrderBy(r => r.Id))
        {
          var registersForReading = registers.Where(r => r.ReadingId == reading.Id).ToList();

          InsertReadingAndRegisters<TSrcReading, TDstReading>(label, reading, registersForReading);

          result = true;
        }
      }
      return result;
    }

    private IList<IDbRegister> GetRegisters<TSrcReading, TSrcRegister>(string label, IEnumerable<TSrcReading> readingsToPipe)
      where TSrcReading : class, IDbReading
      where TSrcRegister : class, IDbRegister
    {
      var tableName = GetTableName<TSrcRegister>();

      log.DebugFormat("Querying for registers for {0}", label);

      var sqlQuery = "SELECT * FROM {0} WHERE ReadingId IN ({1});";
      var ids = string.Join(", ", readingsToPipe.Select(r => r.Id.ToString(CultureInfo.InvariantCulture)));
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, tableName, ids);
      var resultSet = DbContext.QueryTransaction<TSrcRegister>("GetRegisters", sqlQuery);
      log.Debug("Finished query for registers");
      return resultSet.Cast<IDbRegister>().ToList();
    }

    private void InsertReadingAndRegisters<TSrcReading, TDstReading>(string label, TSrcReading reading, ICollection<IDbRegister> registers) 
      where TSrcReading : class, IDbReading, new()
      where TDstReading : class, IDbReading, new()
    {
      var dstReadingTable = GetTableName<TDstReading>();
      var dstRegisterTable = dstReadingTable.Replace("Reading", "Register");

      var transaction = DbContext.BeginTransaction();
      try
      {
        var sql = "INSERT INTO {0} (Label, SerialNumber, Timestamp) VALUES (@Label, @SerialNumber, @Timestamp); SELECT last_insert_rowid();";
        sql = string.Format(CultureInfo.InvariantCulture, sql, dstReadingTable);
        var dstReading = ToDstReading<TDstReading>(reading);
        dstReading.Id = DbContext.Connection.QueryFirst<long>(sql, dstReading, transaction);

        foreach (var register in registers)
        {
          register.ReadingId = dstReading.Id;
        }
        sql = "INSERT INTO {0} (ReadingId,ObisCode,Value,Scale,Unit) VALUES (@ReadingId,@ObisCode,@Value,@Scale,@Unit);";
        sql = string.Format(CultureInfo.InvariantCulture, sql, dstRegisterTable);
        DbContext.Connection.Execute(sql, registers, transaction);

        // Upsert StreamPosition
        sql = "UPDATE StreamPosition SET Position=@Position WHERE StreamName=@StreamName AND Label=@Label;";
        var affectedRecords = DbContext.Connection.Execute(sql, new { Position = reading.Id, StreamName = dstReadingTable, Label = label }, transaction);
        if (affectedRecords == 0)
        {
          var streamPosition = new Db.StreamPosition { StreamName = dstReadingTable, Label = label, Position = reading.Id };
          sql = "INSERT INTO StreamPosition (StreamName,Label,Position) VALUES (@StreamName, @Label, @Position);";
          DbContext.Connection.Execute(sql, streamPosition, transaction);
        }

        transaction.Commit();

        log.DebugFormat("Piped {0}s for {1} time {2} with {3} registers. Set stream position to {4}", 
          dstReadingTable, reading.Label, reading.Timestamp.ToString("o"), registers.Count, reading.Id);
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
        Label = srcReading.Label,
        SerialNumber = srcReading.SerialNumber,
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

