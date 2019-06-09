using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model.Repository
{
  /// <summary>
  /// Performs poor mans backup of the database file by using file copying.
  /// Applies this strategy becasue the Mono.Data.Sqlite assembly does not support
  /// the online backup api.
  /// </summary>
  internal class DbBackup : IDbBackup
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    internal const string BackupPath = "DbBackup";
    private readonly string dbName;
    private readonly TimeSpan minimumTimeSpan;
    private readonly int maxBackupCount;

    public DbBackup(string dbName, TimeSpan minimumTimeSpan, int maxBackupCount)
    {
      if ( string.IsNullOrEmpty(dbName) ) throw new ArgumentNullException("dbName");
      if ( minimumTimeSpan.TotalDays < 14 ) throw new ArgumentOutOfRangeException("minimumTimeSpan", "Must be minimum 14 days");
      if ( maxBackupCount < 1 || maxBackupCount > 10 ) throw new ArgumentOutOfRangeException("maxBackupCount", "Must be between 1 and 10");

      this.dbName = dbName;
      this.minimumTimeSpan = minimumTimeSpan;
      this.maxBackupCount = maxBackupCount;
    }

    public void BackupDatabaseAsNeeded(bool force)
    {
      var dbPath = GetDbPath();
      var dbFile = Path.GetFileName(dbName);

      var backupPath = new DirectoryInfo(Path.Combine(dbPath, BackupPath));
      if (!backupPath.Exists)
      {
        try
        {
          backupPath.Create();
        }
        catch (IOException e)
        {
          log.Error("Failed to create databaes backup directory. Skipping database backup.", e);
          return;
        }
      }

      BackupAsNeeded(force, dbPath, dbFile, backupPath);
      RemoveObsoleteBackup(dbFile, backupPath);
    }

    private string GetDbPath()
    {
      var dbPath = Path.GetDirectoryName(dbName);
      if (dbPath == string.Empty)
      {
        dbPath = Environment.CurrentDirectory;
      }
      return dbPath;
    }

    private void BackupAsNeeded(bool force, string dbPath, string dbFile, DirectoryInfo backupPath)
    {
      var backupFilesAscending = backupPath.GetFiles("*" + dbFile, SearchOption.TopDirectoryOnly).OrderBy(f => f.Name).ToArray();

      if (force || !backupFilesAscending.Any() || DateTime.Now - GetMostRecentBackup(backupFilesAscending.Last().Name) > minimumTimeSpan)
      {
        log.InfoFormat("Backing up database to {0}", backupPath.FullName);
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
            log.Error("Failed to copy database files to backup directory. Skipping database backup.", e);
            return;
          }
          catch (UnauthorizedAccessException e)
          {
            log.Error("Failed to copy database files to backup directory. Skipping database backup.", e);
            return;
          }
        }
        log.InfoFormat("Database backup complete");
      }
    }

    private static DateTime GetMostRecentBackup(string backupFileName)
    {
      return DateTime.ParseExact(backupFileName.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private void RemoveObsoleteBackup(string dbFile, DirectoryInfo backupPath)
    {
      var backupFilesAscending = backupPath.GetFiles("*" + dbFile, SearchOption.TopDirectoryOnly).OrderBy(f => f.Name).ToArray();
      if (backupFilesAscending.Length > maxBackupCount)
      {
        var obsoleteBackup = backupFilesAscending.First();
        foreach (var backupFile in new DirectoryInfo(obsoleteBackup.DirectoryName).GetFiles(obsoleteBackup.Name + "*", SearchOption.TopDirectoryOnly))
        {
          log.DebugFormat("Removing obsolete database backup file:{0}", backupFile.FullName);
          try
          {
            backupFile.Delete();
          }
          catch (IOException e)
          {
            log.Warn("Failed to delete database files from backup directory. Database files may be accumulating.", e);
            return;
          }
          catch (UnauthorizedAccessException e)
          {
            log.Warn("Failed to delete database files from backup directory. Database files may be accumulating.", e);
            return;
          }
        }
      }
    }
  }
}
