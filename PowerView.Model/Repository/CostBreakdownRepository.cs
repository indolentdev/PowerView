using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
  internal class CostBreakdownRepository : RepositoryBase, ICostBreakdownRepository
  {
    public CostBreakdownRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public ICollection<string> GetCostBreakdownTitles()
    {
      const string sql = @"
SELECT cb.Title 
FROM CostBreakdown cb;";
      return DbContext.QueryTransaction<string>(sql);
    }

    public CostBreakdown GetCostBreakdown(string title)
    {
      if (title == null) throw new ArgumentNullException(nameof(title));

      const string sql = @"
SELECT cb.Id, cb.Title, cb.Currency, cb.Vat, cbe.CostBreakdownId, cbe.FromDate, cbe.ToDate, cbe.Name, cbe.StartTime, cbe.EndTime, cbe.Amount 
FROM CostBreakdown cb LEFT JOIN CostBreakdownEntry cbe ON cb.Id=cbe.CostBreakdownId
ORDER BY cb.Id, cbe.FromDate, cbe.ToDate, cbe.Name;";
      var queryResult = DbContext.QueryOneToManyTransaction<Db.CostBreakdown, Db.CostBreakdownEntry>(sql, splitOn: "CostBreakdownId");

      return queryResult.Select(x => ToCostBreakdown(x.Parent, x.Children)).FirstOrDefault();
    }

    public ICollection<CostBreakdown> GetCostBreakdowns()
    {
      const string sql = @"
SELECT cb.Id, cb.Title, cb.Currency, cb.Vat, cbe.CostBreakdownId, cbe.FromDate, cbe.ToDate, cbe.Name, cbe.StartTime, cbe.EndTime, cbe.Amount 
FROM CostBreakdown cb LEFT JOIN CostBreakdownEntry cbe ON cb.Id=cbe.CostBreakdownId
ORDER BY cb.Id, cbe.FromDate, cbe.ToDate, cbe.Name;";
      var queryResult = DbContext.QueryOneToManyTransaction<Db.CostBreakdown, Db.CostBreakdownEntry>(sql, splitOn:"CostBreakdownId");

      return queryResult.Select(x => ToCostBreakdown(x.Parent, x.Children)).ToList();
    }

    private CostBreakdown ToCostBreakdown(Db.CostBreakdown costBreakdown, IEnumerable<Db.CostBreakdownEntry> entries)
    {
      return new CostBreakdown(costBreakdown.Title, (Unit)costBreakdown.Currency, costBreakdown.Vat, entries.Select(ToCostBreakdownEntry).ToList());
    }

    private CostBreakdownEntry ToCostBreakdownEntry(Db.CostBreakdownEntry entry)
    {
      return new CostBreakdownEntry(entry.FromDate, entry.ToDate, entry.Name, entry.StartTime, entry.EndTime, entry.Amount);
    }

    public void AddCostBreakdown(CostBreakdown costBreakdown)
    {
      if (costBreakdown == null) throw new ArgumentNullException(nameof(costBreakdown));

      using var transaction = DbContext.BeginTransaction();
      try
      {
        var dbCostBreakdown = new Db.CostBreakdown
        {
          Title = costBreakdown.Title,
          Currency = (int)costBreakdown.Currency,
          Vat = costBreakdown.Vat
        };

        var id = DbContext.Connection.QueryFirstOrDefault<long>(@"
        INSERT INTO CostBreakdown (Title,Currency,Vat) VALUES (@Title,@Currency,@Vat);
        SELECT LAST_INSERT_ROWID() AS [Id];", dbCostBreakdown, transaction);

        DbContext.Connection.Execute("INSERT INTO CostBreakdownEntry (CostBreakdownId,FromDate,ToDate,Name,StartTime,EndTime,Amount) VALUES (@CostBreakdownId,@FromDate,@ToDate,@Name,@StartTime,@EndTime,@Amount);",
          costBreakdown.Entries.Select(x =>
            new Db.CostBreakdownEntry { CostBreakdownId = id, FromDate = x.FromDate, ToDate = x.ToDate, Name = x.Name, StartTime = x.StartTime, EndTime = x.EndTime, Amount = x.Amount }), transaction);

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void DeleteCostBreakdown(string title)
    {
      if (title == null) throw new ArgumentNullException(nameof(title));

      var sql = @"
        DELETE FROM CostBreakdownEntry
        WHERE CostBreakdownId IN (
          SELECT Id 
          FROM CostBreakdown
          WHERE Title=@title
        );
        DELETE FROM CostBreakdown WHERE Title=@title;";
      DbContext.ExecuteTransaction(sql, new { title });
    }

    public void AddCostBreakdownEntry(string title, CostBreakdownEntry costBreakdownEntry)
    {
      if (title == null) throw new ArgumentNullException(nameof(title));
      if (costBreakdownEntry == null) throw new ArgumentNullException(nameof(costBreakdownEntry));

      using var transaction = DbContext.BeginTransaction();
      try
      {
        var costBreakdown = DbContext.Connection.QueryFirstOrDefault<Db.CostBreakdown>(
            "SELECT Id, Title, Currency, Vat FROM CostBreakdown WHERE Title=@title;", new { title }, transaction);

        if (costBreakdown == null) throw new DataStoreException("CostBreakdown does not exist");

        DbContext.Connection.Execute("INSERT INTO CostBreakdownEntry (CostBreakdownId,FromDate,ToDate,Name,StartTime,EndTime,Amount) VALUES (@CostBreakdownId,@FromDate,@ToDate,@Name,@StartTime,@EndTime,@Amount);",
          new Db.CostBreakdownEntry { CostBreakdownId = costBreakdown.Id, FromDate = costBreakdownEntry.FromDate, ToDate = costBreakdownEntry.ToDate, Name = costBreakdownEntry.Name, StartTime = costBreakdownEntry.StartTime, EndTime = costBreakdownEntry.EndTime, Amount = costBreakdownEntry.Amount }, transaction);

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void UpdateCostBreakdownEntry(string title, DateTime fromDate, DateTime toDate, string name, CostBreakdownEntry costBreakdownEntry)
    {
      if (title == null) throw new ArgumentNullException(nameof(title));
      if (fromDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(fromDate), $"Must be UTC. Was:{fromDate.Kind}");
      if (fromDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(toDate), $"Must be UTC. Was:{fromDate.Kind}");
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (costBreakdownEntry == null) throw new ArgumentNullException(nameof(costBreakdownEntry));

      using var transaction = DbContext.BeginTransaction();
      try
      {
        var costBreakdown = DbContext.Connection.QueryFirstOrDefault<Db.CostBreakdown>(
            "SELECT Id, Title, Currency, Vat FROM CostBreakdown WHERE Title=@title;", new { title }, transaction);

        if (costBreakdown == null) throw new DataStoreException("CostBreakdown does not exist");

        var affected = DbContext.Connection.Execute("UPDATE CostBreakdownEntry SET FromDate=@FromDate, ToDate=@ToDate, Name=@Name, StartTime=@StartTime, EndTime=@EndTime, Amount=@Amount WHERE CostBreakdownId=@CostBreakdownId AND FromDate=@OldFromDate AND ToDate=@OldToDate AND Name=@OldName;",
          new { CostBreakdownId = costBreakdown.Id, OldFromDate = (UnixTime)fromDate, OldToDate= (UnixTime)toDate, OldName=name, FromDate = (UnixTime)costBreakdownEntry.FromDate, ToDate = (UnixTime)costBreakdownEntry.ToDate, Name = costBreakdownEntry.Name, StartTime = costBreakdownEntry.StartTime, EndTime = costBreakdownEntry.EndTime, Amount = costBreakdownEntry.Amount }, transaction);

        if (affected == 0) throw new DataStoreException("CostBreakdownEntry does not exist");

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void DeleteCostBreakdownEntry(string title, DateTime fromDate, DateTime toDate, string name)
    {
      if (title == null) throw new ArgumentNullException(nameof(title));
      if (fromDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(fromDate), $"Must be UTC. Was:{fromDate.Kind}");
      if (fromDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(toDate), $"Must be UTC. Was:{fromDate.Kind}");
      if (name == null) throw new ArgumentNullException(nameof(name));

      using var transaction = DbContext.BeginTransaction();
      try
      {
        var costBreakdown = DbContext.Connection.QueryFirstOrDefault<Db.CostBreakdown>(
            "SELECT Id, Title, Currency, Vat FROM CostBreakdown WHERE Title=@title;", new { title }, transaction);

        if (costBreakdown == null) throw new DataStoreException("CostBreakdown does not exist");

        var affected = DbContext.Connection.Execute("DELETE FROM CostBreakdownEntry WHERE CostBreakdownId=@CostBreakdownId AND FromDate=@FromDate AND ToDate=@ToDate AND Name=@Name;",
          new { CostBreakdownId = costBreakdown.Id, FromDate = (UnixTime)fromDate, ToDate = (UnixTime)toDate, Name = name }, transaction);

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

  }
}
