using System;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class DbContextFactory : IDbContextFactory
  {
    private readonly SQLiteConnectionStringBuilder connectionStringBuilder;

    public DbContextFactory(string dbName)
    {
      if ( string.IsNullOrEmpty(dbName) ) throw new ArgumentNullException("dbName");

      var builder = new SQLiteConnectionStringBuilder();
      builder.BinaryGUID = true;
      builder.DataSource = dbName;
      builder.DateTimeFormat = SQLiteDateFormats.UnixEpoch;
      builder.DateTimeKind = DateTimeKind.Utc;
      builder.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
      builder.DefaultTimeout = 10; //000;
      builder.Enlist = false;
      builder.FailIfMissing = false;
//      builder.ForeignKeys = true;
      builder.JournalMode = SQLiteJournalModeEnum.Persist;
      builder.LegacyFormat = false;
      builder.SyncMode = SynchronizationModes.Normal;
      builder.UseUTF16Encoding = false;
      builder.Version = 3;
      builder.Pooling = false;

      connectionStringBuilder = builder;
    }

    public IDbContext CreateContext()
    {
      var conn = new SQLiteConnection(connectionStringBuilder.ToString());
      try
      {
        conn.Open();
        conn.Execute("PRAGMA foreign_keys = ON");
        return new DbContext(conn);
      }
      catch (SQLiteException e)
      {
        conn.Close();
        conn.Dispose();
        throw DataStoreExceptionFactory.Create(e, "Database open failed");
      }
      catch (Exception)
      {
        conn.Close();
        conn.Dispose();
        throw;
      }
    }

  }
}

