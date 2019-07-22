using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using Dapper;
using DapperExtensions;
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
        var sqlQuery = @"
SELECT MAX(Rank) AS MaxRank
FROM ProfileGraph
WHERE Period=@period AND Page=@page;";
        var dbMaxRank = DbContext.Connection.Query<long?>(sqlQuery, new { period = profileGraph.Period, page = profileGraph.Page }, transaction).FirstOrDefault();
        var newRank = dbMaxRank != null ? dbMaxRank.Value + 1 : 1;

        var dbProfileGraph = new Db.ProfileGraph { Period = profileGraph.Period, Page = profileGraph.Page, 
          Title = profileGraph.Title, Interval = profileGraph.Interval, Rank = profileGraph.Rank };
        if (dbProfileGraph.Rank == 0)
        {
          dbProfileGraph.Rank = newRank;
        }
        log.DebugFormat("Creating ProfileGraphSerie; Period:{0},Page:{1},Rank:{2}", dbProfileGraph.Period, dbProfileGraph.Page, dbProfileGraph.Rank);

        DbContext.Connection.Insert(dbProfileGraph, transaction);
        DbContext.Connection.Insert<Db.ProfileGraphSerie>(profileGraph.SerieNames.Select(x => 
          new Db.ProfileGraphSerie { Label = x.Label, ObisCode = x.ObisCode, ProfileGraphId = dbProfileGraph.Id }), transaction);
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
