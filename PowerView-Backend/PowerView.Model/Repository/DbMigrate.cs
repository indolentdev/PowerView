using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class DbMigrate : RepositoryBase, IDbMigrate
  {
    private readonly ILogger<DbMigrate> logger;

    public DbMigrate(ILogger<DbMigrate> logger, IDbContext dbContext)
      : base(dbContext)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Migrate()
    {
      var currentVersion = GetCurrentVersion();

      if (currentVersion < 6 || currentVersion > 8)
      {
        return;
      }

      Version6Migrate();
    }
      
    private long GetCurrentVersion()
    {
      long currentVersion = 0;
      var versionTablePresenceResult = DbContext.Connection.Query("SELECT COUNT(*) AS VersionPresent FROM sqlite_master WHERE type='table' AND name='Version';");
      if (versionTablePresenceResult.First().VersionPresent > 0)
      {
        var versionNumberResult = DbContext.Connection.Query("SELECT MAX(Number) AS Number FROM Version");
        dynamic versionNumberRow = versionNumberResult.First(); 
        if ( versionNumberRow.Number != null )
        {
          currentVersion = versionNumberRow.Number;
        }
      }
      else
      {
        throw new InvalidOperationException("No Version table in the database");
      }
      return currentVersion;
    }

    private void Version6Migrate()
    {
      var labelsToMigrate = DbContext.Connection.Query("SELECT DISTINCT Label FROM LiveReading WHERE SerialNumber is NULL;")
        .Select(row => (string)row.Label).ToArray();
      if (labelsToMigrate.Length == 0)
      {
        logger.LogInformation("Migration of data for database schema version 6 already complete");
        return;
      }

      var labelsAndSerialNumbers = DbContext.Connection.Query("SELECT * FROM LiveReading WHERE SerialNumber IS NOT NULL GROUP BY Label;")
        .Select(row => new { Label = (string)row.Label, SerialNumber= (long)row.SerialNumber })
        .Where(item => labelsToMigrate.Contains(item.Label)).ToArray();
      if (labelsAndSerialNumbers.Length == 0)
      {
        return;
      }

      logger.LogInformation("Migrating data for database schema version 6");

      const string sql = "UPDATE LiveReading SET SerialNumber=@SerialNumber WHERE Label=@Label AND SerialNumber IS NULL";
      foreach (var item in labelsAndSerialNumbers)
      {
        var tran = DbContext.BeginTransaction();
        try
        {
          DbContext.Connection.Execute(sql, item, tran); 
          tran.Commit();
        }
        catch (SqliteException)
        {
          tran.Rollback();
          logger.LogWarning("Failed migrating data. Retrying next time");
          return;
        }
      }

    }

  }
}
