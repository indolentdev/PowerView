using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class DbUpgrade : RepositoryBase, IDbUpgrade
    {
        private readonly ILogger logger;

        public DbUpgrade(ILogger<DbUpgrade> logger, IDbContext dbContext)
          : base(dbContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            bool upgradesPerformed = false;

            var asm = Assembly.GetExecutingAssembly();
            foreach (var dbUpgradeResource in dbUpgradeResources)
            {
                using (var ddlResourceReader = new StreamReader(asm.GetManifestResourceStream(dbUpgradeResource.ResourceName)))
                {
                    var newVersion = new { Number = long.MaxValue, Timestamp = (UnixTime)DateTime.UtcNow };
                    DbContext.ExecuteTransaction("INSERT INTO Version (Number, Timestamp) VALUES (@Number, @Timestamp);", newVersion);

                    logger.LogInformation($"Applying database schema update for version {dbUpgradeResource.Version}");
                    ApplyDdlScript(dbUpgradeResource.ResourceName, ddlResourceReader.ReadToEnd());

                    DbContext.ExecuteTransaction("UPDATE Version SET Number = @NewNumber WHERE Number = @OldNumber AND Timestamp = @Timestamp;",
                      new { OldNumber = newVersion.Number, newVersion.Timestamp, NewNumber = dbUpgradeResource.Version });

                    logger.LogInformation("Database schema update complete");

                    upgradesPerformed = true;
                }
            }

            if (upgradesPerformed)
            {
                logger.LogInformation("Performing database cleanup after schema update.");
                DbContext.ExecuteNoTransaction("VACUUM;");
                logger.LogInformation("Database cleanup completed.");
            }
        }

        private long GetCurrentVersion()
        {
            long currentVersion = 0;
            var versionTablePresenceResult = DbContext.Connection.Query("SELECT COUNT(*) AS VersionPresent FROM sqlite_master WHERE type='table' AND name='Version';");
            if (versionTablePresenceResult.First().VersionPresent > 0)
            {
                var versionNumberResult = DbContext.Connection.Query("SELECT MAX(Number) AS Number FROM Version");
                dynamic versionNumberRow = versionNumberResult.First();
                if (versionNumberRow.Number != null)
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
            var lines = ddl.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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

        private IEnumerable<DbUpgradeResource> GetUpgradeManifestResources(long currentVersion)
        {
            var resources = GetUpgradeManifestResources().OrderBy(i => i.Version).ToList();
            var applicationExpectedVersion = resources.Last().Version;
            if (currentVersion > applicationExpectedVersion)
            {
                logger.LogWarning($"Database schema version greater than expected. Was:{currentVersion}. Expected:{applicationExpectedVersion}. PowerView may not function. It may help upgrading to a newer PowerView version.");
            }
            return resources.Where(i => i.Version > currentVersion);
        }

        private IEnumerable<DbUpgradeResource> GetUpgradeManifestResources()
        {
            var type = MethodBase.GetCurrentMethod().DeclaringType;
            var startResourceName = type.Namespace + ".DbVersion";
            var resourceNameVersionIndex = startResourceName.Split(new[] { '.' }).Length;
            var resourceNames = type.Assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames.Where(rn => rn.StartsWith(startResourceName, StringComparison.InvariantCulture)))
            {
                var resourceNameElements = resourceName.Split(new[] { '.' });
                var versionElement = resourceNameElements[resourceNameVersionIndex];
                versionElement = versionElement.Replace("_", string.Empty);
                long version;
                if (!long.TryParse(versionElement, NumberStyles.Integer, CultureInfo.InvariantCulture, out version))
                {
                    logger.LogWarning($"Unable to parse db version element from resource upgrade name. Skipping resource. ResourceName:{resourceName}, VersionElement:{versionElement}");
                    continue;
                }
                yield return new DbUpgradeResource(version, resourceName);
            }
        }

        private class DbUpgradeResource
        {
            public DbUpgradeResource(long version, string resourceName)
            {
                if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException("resourceName");

                Version = version;
                ResourceName = resourceName;
            }

            public long Version { get; private set; }
            public string ResourceName { get; private set; }
        }
    }
}

