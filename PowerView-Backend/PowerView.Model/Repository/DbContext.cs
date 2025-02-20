using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Reflection;

namespace PowerView.Model.Repository
{
    internal class DbContext : IDbContext
    {
        private static readonly DateTime dateTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private const int CommandTimeout = 10;

        private readonly IDbConnection connection;

        public DbContext(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

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

        internal int ExecuteTransaction(string sql, object param = null)
        {
            return InTransaction(transaction => connection.Execute(sql, param, transaction, CommandTimeout));
        }

        internal int ExecuteNoTransaction(string sql, object param = null)
        {
            return NoTransaction(() => connection.Execute(sql, param, null, CommandTimeout));
        }

        internal IList<TReturn> QueryTransaction<TReturn>(string sql, object param = null)
        {
            return InTransaction(transaction => connection.Query<TReturn>(sql, param, transaction, false, CommandTimeout).ToList());
        }

        internal IList<dynamic> QueryTransaction(string sql, object param = null)
        {
            return InTransaction(transaction => connection.Query(sql, param, transaction, false, CommandTimeout).ToList());
        }

        internal void QueryMultiMappingTransaction<T1, T2, T3, TReturn>(string sql, Func<T1, T2, T3, TReturn> map, object param = null, string splitOn = "Id")
        {
            InTransaction(transaction => connection.Query(sql, map, param, transaction, true, splitOn, CommandTimeout));
        }

        internal IList<TReturn> QueryNoTransaction<TReturn>(string sql, object param = null, int? commandTimeout = null)
        {
            var cmdTimeout = commandTimeout != null ? commandTimeout.Value : CommandTimeout;
            return NoTransaction(() => connection.Query<TReturn>(sql, param, null, false, cmdTimeout).ToList());
        }

        internal class OneToMany<TOne, TMany> where TOne : class, IDbEntity where TMany : class
        {
            public OneToMany(TOne parent)
            {
                Parent = parent;
                Children = new List<TMany>();
            }

            public TOne Parent { get; }
            public IList<TMany> Children { get; }
        }

        internal ICollection<OneToMany<TOne, TMany>> QueryOneToManyTransaction<TOne, TMany>(string sql, object param = null, string splitOn = "Id") where TOne : class, IDbEntity where TMany : class
        {
            Dictionary<long, OneToMany<TOne, TMany>> cache = new Dictionary<long, OneToMany<TOne, TMany>>();
            Func<TOne, TMany, OneToMany<TOne, TMany>> map = (parent, child) =>
            {
                if (!cache.ContainsKey(parent.Id))
                {
                    cache.Add(parent.Id, new OneToMany<TOne, TMany>(parent));
                }
                
                var oneToMany = cache[parent.Id];

                if (child != null) oneToMany.Children.Add(child);
                
                return oneToMany;
            };

            InTransaction(transaction => connection.Query(sql, map, param, transaction, true, splitOn, CommandTimeout));

            return cache.Values;
        }

        private TReturn InTransaction<TReturn>(Func<IDbTransaction, TReturn> dbFunc)
        {
            using var transaction = BeginTransaction();
            try
            {
                TReturn ret = dbFunc(transaction);
                transaction.Commit();
                return ret;
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                throw DataStoreExceptionFactory.Create(e);
            }
        }

        private TReturn NoTransaction<TReturn>(Func<TReturn> dbFunc)
        {
            try
            {
                TReturn ret = dbFunc();
                return ret;
            }
            catch (SqliteException e)
            {
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
                connection.Execute("PRAGMA analysis_limit = 200;");
                connection.Execute("PRAGMA optimize;");

                connection.Close();
                connection.Dispose();
            }
            disposed = true;
        }

    }
}

