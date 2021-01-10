using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using Dapper;
using log4net;

namespace PowerView.Model.Repository
{
  internal class ProfileGraphRepository : RepositoryBase, IProfileGraphRepository
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ProfileGraphRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public ICollection<string> GetProfileGraphPages(string period)
    {
      const string sql = @"
SELECT DISTINCT pg.Page
FROM ProfileGraph pg
WHERE pg.Period=@period;";
      return DbContext.QueryTransaction<string>("GetProfileGraphPages", sql, new { period });
    }

    public ICollection<ProfileGraph> GetProfileGraphs()
    {
      const string sql = @"
SELECT pg.Id, pg.Period, pg.Page, pg.Title, pg.Interval, pg.Rank, pgs.Id, pgs.Label, pgs.ObisCode, pgs.ProfileGraphId
FROM ProfileGraph pg JOIN ProfileGraphSerie pgs on pg.Id=pgs.ProfileGraphId
ORDER BY pg.Rank, pgs.Id;";
      var queryResult = DbContext.QueryOneToManyTransaction<Db.ProfileGraph, Db.ProfileGraphSerie>("GetProfileGraphs", sql);

      return queryResult.Select(x => ToProfileGraph(x.Parent, x.Children)).ToList();
    }

    public ICollection<ProfileGraph> GetProfileGraphs(string period, string page)
    {
      const string sql = @"
SELECT pg.Id, pg.Period, pg.Page, pg.Title, pg.Interval, pg.Rank, pgs.Id, pgs.Label, pgs.ObisCode, pgs.ProfileGraphId
FROM ProfileGraph pg JOIN ProfileGraphSerie pgs on pg.Id=pgs.ProfileGraphId
WHERE pg.Period=@period AND pg.Page=@page
ORDER BY pg.Rank, pgs.Id;";
      var queryResult = DbContext.QueryOneToManyTransaction<Db.ProfileGraph, Db.ProfileGraphSerie>("GetProfileGraphs", sql, new { period, page});

      return queryResult.Select(x => ToProfileGraph(x.Parent, x.Children)).ToList();
    }

    private ProfileGraph ToProfileGraph(Db.ProfileGraph profileGraph, IEnumerable<Db.ProfileGraphSerie> series)
    {
      var serieNames = series.Select(ToSerieName).ToList();
      return new ProfileGraph(profileGraph.Period, profileGraph.Page, profileGraph.Title, profileGraph.Interval, profileGraph.Rank, serieNames);
    }

    private SeriesName ToSerieName(Db.ProfileGraphSerie serie)
    {
      return new SeriesName(serie.Label, serie.ObisCode);
    }

    public void AddProfileGraph(ProfileGraph profileGraph)
    {
      var transaction = DbContext.BeginTransaction();
      try
      {
        var newRank = GetNewRank(transaction, profileGraph.Period, profileGraph.Page);

        var dbProfileGraph = new Db.ProfileGraph { Period = profileGraph.Period, Page = profileGraph.Page, 
          Title = profileGraph.Title, Interval = profileGraph.Interval, Rank = profileGraph.Rank };
        if (dbProfileGraph.Rank == 0)
        {
          dbProfileGraph.Rank = newRank;
        }
        log.DebugFormat("Creating ProfileGraph; Period:{0},Page:{1},Rank:{2}", dbProfileGraph.Period, dbProfileGraph.Page, dbProfileGraph.Rank);

        var profileGraphId = DbContext.Connection.QueryFirstOrDefault<long>(@"
INSERT INTO ProfileGraph (Period, Page, Title, Interval, Rank) VALUES (@Period, @Page, @Title, @Interval, @Rank);
SELECT LAST_INSERT_ROWID() AS [Id];", dbProfileGraph, transaction);

        DbContext.Connection.Execute("INSERT INTO ProfileGraphSerie (Label, ObisCode, ProfileGraphId) VALUES (@Label, @ObisCode, @ProfileGraphId);",
          profileGraph.SerieNames.Select(x =>
            new Db.ProfileGraphSerie { Label = x.Label, ObisCode = x.ObisCode, ProfileGraphId = profileGraphId }), transaction);

        transaction.Commit();
        log.InfoFormat("Created ProfileGraph; Period:{0},Page:{1},Title:{2}. Rows affected:{3}",
                        profileGraph.Period, profileGraph.Page, profileGraph.Title, 1+profileGraph.SerieNames.Count);
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public bool UpdateProfileGraph(string period, string page, string title, ProfileGraph profileGraph)
    {
      var setNewRank = true;
      if (string.Equals(period, profileGraph.Period, StringComparison.Ordinal) && string.Equals(page, profileGraph.Page, StringComparison.Ordinal))
      {
        setNewRank = false;
      }

      var transaction = DbContext.BeginTransaction();
      try
      {
        var existing = DbContext.Connection.QueryFirstOrDefault(@"
SELECT Id, Rank FROM ProfileGraph WHERE Period=@period AND Page=@Page AND Title=@title;", new { period, page, title }, transaction);
        if (existing == null)
        {
          return false;
        }

        var rank = (long)existing.Rank;
        if (setNewRank)
        {
          rank = GetNewRank(transaction, profileGraph.Period, profileGraph.Page);
        }

        var dbProfileGraph = new Db.ProfileGraph
        {
          Id = existing.Id,
          Period = profileGraph.Period,
          Page = profileGraph.Page,
          Title = profileGraph.Title,
          Interval = profileGraph.Interval,
          Rank = rank
        };
        log.DebugFormat("Updating ProfileGraph; Period:{0},Page:{1},Title:{2}", period, page, title);

        var pgRowsAffected = DbContext.Connection.Execute(@"
UPDATE ProfileGraph SET Period=@Period, Page=@Page, Title=@Title, Interval=@Interval, Rank=@Rank WHERE Id=@Id;", dbProfileGraph, transaction);

        var pgsDeletedRowsAffected = DbContext.Connection.Execute(@"
DELETE FROM ProfileGraphSerie WHERE ProfileGraphId=@Id;", dbProfileGraph, transaction);

        var pgsAddedRowsAffected = DbContext.Connection.Execute(@"
INSERT INTO ProfileGraphSerie (Label, ObisCode, ProfileGraphId) VALUES (@Label, @ObisCode, @ProfileGraphId);",
          profileGraph.SerieNames.Select(x =>
            new Db.ProfileGraphSerie { Label = x.Label, ObisCode = x.ObisCode, ProfileGraphId = dbProfileGraph.Id }), transaction);

        transaction.Commit();
        log.InfoFormat("Updated ProfileGraph; Period:{0},Page:{1},Title:{2}. Rows affected:{3}", period, page, title, pgRowsAffected + pgsDeletedRowsAffected + pgsAddedRowsAffected);
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }

      return true;
    }

    private long GetNewRank(IDbTransaction transaction, string period, string page)
    {
      var sqlQuery = @"
SELECT MAX(Rank) AS MaxRank
FROM ProfileGraph
WHERE Period=@period AND Page=@page;";
      var dbMaxRank = DbContext.Connection.Query<long?>(sqlQuery, new { period = period, page = page }, transaction).FirstOrDefault();
      var newRank = dbMaxRank != null ? dbMaxRank.Value + 1 : 1;
      return newRank;
    }


    public void DeleteProfileGraph(string period, string page, string title)
    {
      var where = "WHERE Period=@period AND Page=@page AND Title=@title";
      var sql = @"
DELETE FROM ProfileGraphSerie
WHERE ProfileGraphId IN (
  SELECT Id 
  FROM ProfileGraph
  {0}
);
DELETE FROM ProfileGraph {0};";
      sql = string.Format(CultureInfo.InvariantCulture, sql, where);
      var rowsAffected = DbContext.ExecuteTransaction("DeleteProfileGraph", sql, new { period, page, title });
      log.InfoFormat("Deleted ProfileGraph; Period:{0},Section:{1},Title:{2}. Rows affected:{3}", 
                      period, page, title, rowsAffected);
    }

    public void SwapProfileGraphRank(string period, string page, string title1, string title2)
    {
      var sql = @"
CREATE TEMPORARY TABLE temp.RankPlaceholder AS
SELECT Period, Page, Title, Rank FROM ProfileGraph WHERE Period=@period AND Page=@page AND (Title=@title1 OR Title=@title2);

UPDATE ProfileGraph
SET Rank = @tmpRank
WHERE Period=@period AND Page=@page AND Title=@title2;

UPDATE ProfileGraph
SET Rank = (SELECT Rank FROM RankPlaceholder WHERE Period=@period AND Page=@page AND Title=@title2)
WHERE Period=@period AND Page=@page AND Title=@title1; 

UPDATE ProfileGraph
SET Rank = (SELECT Rank FROM RankPlaceholder WHERE Period=@period AND Page=@page AND Title=@title1)
WHERE Period=@period AND Page=@page AND Title=@title2;
";
      var rowsAffected = DbContext.ExecuteTransaction("SwapProfileGraphRank", sql, new { period, page, title1, title2, tmpRank = long.MaxValue });
      log.InfoFormat("Swapped ProfileGraph ranks; Period:{0},Page:{1},Title1:{2},Title2:{3}. Rows affected:{4}",
                      period, page, title1, title2, rowsAffected-2);
    }
  }
}
