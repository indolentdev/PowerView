using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Mono.Data.Sqlite;
using Dapper;
using log4net;
using System.Reflection;

namespace PowerView.Model.Repository
{
  internal class DbContext : IDbContext
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly DateTime dateTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private const int CommandTimeout = 10;

    private readonly IDbConnection connection;

    public DbContext(IDbConnection connection)
    {
      if ( connection == null ) throw new ArgumentNullException("connection");

      this.connection = connection;
    }

    public IDbConnection Connection { get { return connection; } }

    public IDbTransaction BeginTransaction()
    {
      return connection.BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public DateTime GetDateTime(long fieldValue)
    {
      return dateTimeEpoch.AddSeconds(fieldValue);
    }

    internal int ExecuteTransaction(string dbOp, string sql, object param = null)
    {
      return InTransaction(transaction => connection.Execute(sql, param, transaction, CommandTimeout), dbOp);
    }

    internal int ExecuteNoTransaction(string dbOp, string sql, object param = null)
    {
      return NoTransaction(() => connection.Execute(sql, param, null, CommandTimeout), dbOp);
    }

    internal IList<TReturn> QueryTransaction<TReturn>(string dbOp, string sql, object param = null)
    {
      return InTransaction(transaction => connection.Query<TReturn>(sql, param, transaction, false, CommandTimeout).ToList(), dbOp);
    }

    internal IList<dynamic> QueryTransaction(string dbOp, string sql, object param = null)
    {
      return InTransaction(transaction => connection.Query(sql, param, transaction, false, CommandTimeout).ToList(), dbOp);
    }

    internal IList<TReturn> QueryNoTransaction<TReturn>(string dbOp, string sql, object param = null, int? commandTimeout = null)
    {
      var cmdTimeout = commandTimeout != null ? commandTimeout.Value : CommandTimeout;
      return NoTransaction(() => connection.Query<TReturn>(sql, param, null, false, cmdTimeout).ToList(), dbOp);
    }

    internal class OneToMany<TOne, TMany> where TOne : class, IDbEntity where TMany : class, IDbEntity
    {
      public OneToMany(TOne parent)
      {
        Parent = parent;
        Children = new List<TMany>();
      }

      public TOne Parent { get; }
      public IList<TMany> Children { get; }
    }

    internal ICollection<OneToMany<TOne, TMany>> QueryOneToManyTransaction<TOne, TMany>(string dbOp, string sql, object param = null) where TOne : class, IDbEntity where TMany : class, IDbEntity
    {
      Dictionary<long, OneToMany<TOne, TMany>> cache = new Dictionary<long, OneToMany<TOne, TMany>>();
      Func<TOne, TMany, OneToMany<TOne, TMany>> map = (parent, child) =>
      {
        if (!cache.ContainsKey(parent.Id))
        {
          cache.Add(parent.Id, new OneToMany<TOne, TMany>(parent));
        }

        var oneToMany = cache[parent.Id];
        oneToMany.Children.Add(child);
        return oneToMany;
      };

      InTransaction(transaction => connection.Query(sql, map, param, transaction, true, "Id", CommandTimeout), dbOp);

      return cache.Values;
    }

    private TReturn InTransaction<TReturn>(Func<IDbTransaction, TReturn> dbFunc, string dbOp = null)
    {
      log.DebugFormat("Starting database operation " + dbOp);
      using (var transaction = BeginTransaction())
      { 
        try
        {
          TReturn ret = dbFunc(transaction);
          transaction.Commit();
          log.DebugFormat("Finished database operation. Ok");
          return ret;
        }
        catch (SqliteException e)
        {
          log.DebugFormat("Finished database operation. Error");
          transaction.Rollback();
          throw DataStoreExceptionFactory.Create(e);
        }
      }
    }

    private TReturn NoTransaction<TReturn>(Func<TReturn> dbFunc, string dbOp = null)
    {
      log.DebugFormat("Starting database operation " + dbOp);
      try
      {
        TReturn ret = dbFunc();
        log.DebugFormat("Finished database operation. Ok");
        return ret;
      }
      catch (SqliteException e)
      {
        log.DebugFormat("Finished database operation. Error");
        throw DataStoreExceptionFactory.Create(e);
      }
    }


    public void Dispose() 
    {
      Dispose(true);
      GC.SuppressFinalize(this);      
    }

    ~DbContext()
    {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }

    private bool disposed;
    protected virtual void Dispose(bool disposing)
    {
      if (disposed)
      {
        return;
      }

      if (disposing) 
      {
        connection.Close();
        connection.Dispose();
      }
      disposed = true;   
    }

  }
}

