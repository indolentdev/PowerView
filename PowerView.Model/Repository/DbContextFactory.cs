using System;
using System.Data;
using System.Data.SQLite;
using Microsoft.Extensions.Options;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class DbContextFactory : IDbContextFactory
    {
        private readonly SQLiteConnectionStringBuilder connectionStringBuilder;

        public DbContextFactory(IOptions<DatabaseOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var builder = new SQLiteConnectionStringBuilder();
            builder.BinaryGUID = true;
            builder.DataSource = options.Value.Name;
            builder.DateTimeFormat = SQLiteDateFormats.UnixEpoch;
            builder.DateTimeKind = DateTimeKind.Utc;
            builder.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
            builder.DefaultTimeout = 10; //000;
            builder.Enlist = false;
            builder.FailIfMissing = false;
            builder.ForeignKeys = true;
            builder.JournalMode = SQLiteJournalModeEnum.Delete;
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

