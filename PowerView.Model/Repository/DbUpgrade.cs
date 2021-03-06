using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using Mono.Data.Sqlite;
using log4net;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class DbUpgrade : RepositoryBase, IDbUpgrade
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DbUpgrade(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public bool IsNeeded()
    {
      return GetDbUpgradeResources().Any();
    }

    public void ApplyUpdates()
    {
      var dbUpgradeResources = GetDbUpgradeResources();

      ApplySchemaUpdates(dbUpgradeResources);
    }

    private IEnumerable<DbUpgradeResource> GetDbUpgradeResources()
    {
      var currentVersion = GetCurrentVersion();

      var dbUpgradeResources = GetUpgradeManifestResources(currentVersion);

      return dbUpgradeResources;
    }

    private void ApplySchemaUpdates(IEnumerable<DbUpgradeResource> dbUpgradeResources)
    {
      var asm = Assembly.GetExecutingAssembly();
      foreach (var dbUpgradeResource in dbUpgradeResources)
      {
        using (var ddlResourceReader = new StreamReader(asm.GetManifestResourceStream(dbUpgradeResource.ResourceName)))
        {
          var newVersion = new { Number = long.MaxValue, Timestamp = DateTime.UtcNow };
          DbContext.ExecuteTransaction("ApplySchemaUpdates", "INSERT INTO Version (Number, Timestamp) VALUES (@Number, @Timestamp);", newVersion);

          log.InfoFormat("Applying database schema update for version {0}", dbUpgradeResource.Version);
          ApplyDdlScript(dbUpgradeResource.ResourceName, ddlResourceReader.ReadToEnd());

          DbContext.ExecuteTransaction("ApplySchemaUpdates", "UPDATE Version SET Number = @NewNumber WHERE Number = @OldNumber AND Timestamp = @Timestamp;",
            new { OldNumber = newVersion.Number, newVersion.Timestamp, NewNumber = dbUpgradeResource.Version });
          log.Info("Database schema update complete");
        }
      }
      DbContext.ExecuteNoTransaction("ApplySchemaUpdates-VACUUM", "VACUUM;");
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
        DbContext.Connection.Execute("CREATE TABLE Version (Id INTEGER PRIMARY KEY, Number INTEGER NOT NULL, Timestamp DATETIME NOT NULL);");
      }
      if (currentVersion == long.MaxValue)
      {
        throw new InvalidOperationException("Database schema version corrupted. Restore a previous backup.");
      }
      return currentVersion;
    }

    private void ApplyDdlScript(string upgradeResourceName, string ddl)
    {
      var lines = ddl.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
      var lineNumber = 0;
      foreach (var line in lines)
      {
        lineNumber++;
        if (string.IsNullOrEmpty(line))
        {
          continue;
        }

        try
        {
          DbContext.Connection.Execute(line.Trim());
        }
        catch (SqliteException e)
        {
          var msg = string.Format(CultureInfo.InvariantCulture, "Failed executing database upgrade script {0} at line {1}: {2}", upgradeResourceName, lineNumber, line);
          throw DataStoreExceptionFactory.Create(e, msg);
        }
      }
    }

    private static IEnumerable<DbUpgradeResource> GetUpgradeManifestResources(long currentVersion)
    {
      var resources = GetUpgradeManifestResources().OrderBy(i => i.Version).ToList();
      var applicationExpectedVersion = resources.Last().Version;
      if (currentVersion > applicationExpectedVersion)
      {
        log.WarnFormat(CultureInfo.InvariantCulture, 
          "Database schema version greater than expected. Was:{0}. Expected:{1}. PowerView may not function. It may help upgrading to a newer PowerView version.",
          currentVersion, applicationExpectedVersion);
      }
      return resources.Where(i => i.Version > currentVersion);
    }

    private static IEnumerable<DbUpgradeResource> GetUpgradeManifestResources()
    {
      var type = MethodBase.GetCurrentMethod().DeclaringType;
      var startResourceName = type.Namespace + ".DbVersion";
      var resourceNameVersionIndex = startResourceName.Split(new [] { '.' }).Length;
      var resourceNames = type.Assembly.GetManifestResourceNames();

      foreach (var resourceName in resourceNames.Where(rn => rn.StartsWith(startResourceName, StringComparison.InvariantCulture)))
      {
        var resourceNameElements = resourceName.Split(new [] { '.' });
        var versionElement = resourceNameElements[resourceNameVersionIndex];
        versionElement = versionElement.Replace("_", string.Empty);
        long version;
        if (!long.TryParse(versionElement, NumberStyles.Integer, CultureInfo.InvariantCulture, out version))
        {
          log.ErrorFormat(CultureInfo.InvariantCulture, "Unable to parse db version element from resource upgrade name. Skipping resource. ResourceName:{0}, VersionElement:{1}",
                          resourceName, versionElement);
          continue;
        }
        yield return new DbUpgradeResource(version, resourceName);
      }
    }

    private class DbUpgradeResource
    {
      public DbUpgradeResource(long version, string resourceName)
      {
        if ( string.IsNullOrEmpty(resourceName) ) throw new ArgumentNullException("resourceName");

        Version = version;
        ResourceName = resourceName;
      }

      public long Version { get; private set; }
      public string ResourceName { get; private set; }
    }
  }
}

