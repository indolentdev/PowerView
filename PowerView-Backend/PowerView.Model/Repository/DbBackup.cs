using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PowerView.Model.Repository
{
    /// <summary>
    /// Performs poor mans backup of the database file by using file copying.
    /// Applies this strategy becasue the Mono.Data.Sqlite assembly does not support
    /// the online backup api.
    /// </summary>
    internal class DbBackup : IDbBackup
    {
        internal const string BackupPath = "DbBackup";
        private readonly ILogger logger;
        private readonly IOptions<DatabaseOptions> dbOptions;
        private readonly IOptions<DatabaseBackupOptions> bckOptions;

        public DbBackup(ILogger<DbBackup> logger, IOptions<DatabaseOptions> dbOptions, IOptions<DatabaseBackupOptions> bckOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbOptions = dbOptions ?? throw new ArgumentNullException(nameof(dbOptions));
            this.bckOptions = bckOptions ?? throw new ArgumentNullException(nameof(bckOptions));
        }

        public void BackupDatabaseAsNeeded(bool force)
        {
            var dbPath = GetDbPath();
            var dbFile = Path.GetFileName(dbOptions.Value.Name);

            var backupPath = new DirectoryInfo(Path.Combine(dbPath, BackupPath));
            if (!backupPath.Exists)
            {
                try
                {
                    backupPath.Create();
                }
                catch (IOException e)
                {
                    logger.LogError(e, "Failed to create databaes backup directory. Skipping database backup.");
                    return;
                }
            }

            BackupAsNeeded(force, dbPath, dbFile, backupPath);
            RemoveObsoleteBackup(dbFile, backupPath);
        }

        private string GetDbPath()
        {
            var dbPath = Path.GetDirectoryName(dbOptions.Value.Name);
            if (dbPath == string.Empty)
            {
                dbPath = Environment.CurrentDirectory;
            }
            return dbPath;
        }

        private void BackupAsNeeded(bool force, string dbPath, string dbFile, DirectoryInfo backupPath)
        {
            if (force || TimeForBackup(dbFile, backupPath))
            {
                logger.LogInformation("Backing up database to file:{Name}", backupPath.FullName);
                var dt = DateTime.Now;
                var backupTime = dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                foreach (var sourceFile in new DirectoryInfo(dbPath).GetFiles(dbFile + "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        sourceFile.CopyTo(Path.Combine(backupPath.FullName, backupTime + "_" + sourceFile.Name));
                    }
                    catch (IOException e)
                    {
                        logger.LogError(e, "Failed to copy database files to backup directory. Skipping database backup.");
                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        logger.LogError(e, "Failed to copy database files to backup directory. Skipping database backup.");
                        return;
                    }
                }
                logger.LogInformation("Database backup complete");
            }
        }

        private bool TimeForBackup(string dbFile, DirectoryInfo backupPath)
        {
            var backupFiles = backupPath.GetFiles("*" + dbFile, SearchOption.TopDirectoryOnly).ToArray();

            var managedBackupFiles = backupFiles
                .Select(x =>
                {
                    var managedBackup = TryGetDateTimeFromFileName(x.Name, out var dateTime);
                    return new { FileInfo = x, ManagedBackup = managedBackup, DateTime = dateTime };
                })
                .Where(x => x.ManagedBackup)
                .OrderBy(x => x.DateTime)
                .ToList();

            return managedBackupFiles.Count == 0 || DateTime.Now - managedBackupFiles.Last().DateTime > bckOptions.Value.MinimumInterval;
        }

        private static bool TryGetDateTimeFromFileName(string backupFileName, out DateTime dateTime)
        {
            return DateTime.TryParseExact(backupFileName.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
        }

        private void RemoveObsoleteBackup(string dbFile, DirectoryInfo backupPath)
        {
            var backupFilesAscending = backupPath.GetFiles("*" + dbFile, SearchOption.TopDirectoryOnly).OrderBy(f => f.Name).ToArray();
            if (backupFilesAscending.Length > bckOptions.Value.MaximumCount)
            {
                var obsoleteBackup = backupFilesAscending.First();
                foreach (var backupFile in new DirectoryInfo(obsoleteBackup.DirectoryName).GetFiles(obsoleteBackup.Name + "*", SearchOption.TopDirectoryOnly))
                {
                    logger.LogDebug("Removing obsolete database backup file:{Name}", backupFile.FullName);
                    try
                    {
                        backupFile.Delete();
                    }
                    catch (IOException e)
                    {
                        logger.LogWarning(e, "Failed to delete database files from backup directory. Database files may be accumulating.");
                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        logger.LogWarning(e, "Failed to delete database files from backup directory. Database files may be accumulating.");
                        return;
                    }
                }
            }
        }
    }
}
