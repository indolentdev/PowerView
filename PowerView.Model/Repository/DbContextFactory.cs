using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class DbContextFactory : IDbContextFactory
    {
        private readonly SqliteConnectionStringBuilder connectionStringBuilder;

        public DbContextFactory(IOptions<DatabaseOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = options.Value.Name;
            builder.DefaultTimeout = 10; //000;
            builder.ForeignKeys = true;
            builder.Mode = SqliteOpenMode.ReadWriteCreate;
            builder.ForeignKeys = true;
            builder.Cache = SqliteCacheMode.Default;
            builder.Pooling = false;

            connectionStringBuilder = builder;
        }

        public IDbContext CreateContext()
        {
            var conn = new SqliteConnection(connectionStringBuilder.ToString());
            try
            {
                conn.Open();
                conn.Execute("PRAGMA journal_mode = DELETE;");
                conn.Execute("PRAGMA encoding = 'UTF-8';");
                conn.Execute("PRAGMA synchronous = NORMAL;");
                return new DbContext(conn);
            }
            catch (SqliteException e)
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

