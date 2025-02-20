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
            
            connectionStringBuilder = GetConnectionStringBuilder(options.Value.Name);
        }

        internal static SqliteConnectionStringBuilder GetConnectionStringBuilder(string dataSource)
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = dataSource;
            builder.DefaultTimeout = 10; //000;
            builder.ForeignKeys = true;
            builder.Mode = SqliteOpenMode.ReadWriteCreate;
            builder.Cache = SqliteCacheMode.Default;
            builder.Pooling = false;
            return builder;
        }

        public IDbContext CreateContext()
        {
            return new DbContext(GetConnection(connectionStringBuilder));
        }

        internal static SqliteConnection GetConnection(SqliteConnectionStringBuilder builder)
        {
            var conn = new SqliteConnection(builder.ToString());
            try
            {
                conn.Open();
                conn.Execute("PRAGMA journal_mode = DELETE;");
                conn.Execute("PRAGMA encoding = 'UTF-8';");
                conn.Execute("PRAGMA synchronous = FULL;");
                return conn;
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

