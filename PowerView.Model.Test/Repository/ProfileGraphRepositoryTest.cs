using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using System.Globalization;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class ProfileGraphRepositoryTest : DbTestFixtureWithSchema
  {
    [SetUp]
    public override void SetUp()
    {
      base.SetUp();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new ProfileGraphRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetProfileGraphPages()
    {
      // Arrange
      var pg1 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t1", Interval = "5-minutes", Rank = 1 };
      var pg2 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t2", Interval = "5-minutes", Rank = 2 };
      var pg3 = new Db.ProfileGraph { Period = "day", Page = "p2", Title = "t1", Interval = "5-minutes", Rank = 3 };
      var pg4 = new Db.ProfileGraph { Period = "day", Page = "", Title = "t1", Interval = "5-minutes", Rank = 4 };
      var pg5 = new Db.ProfileGraph { Period = "month", Page = "p2", Title = "t1", Interval = "1-days", Rank = 5 };
      InsertProfileGraphs(pg1, pg2, pg3, pg4, pg5);
      var target = CreateTarget();

      // Act
      var pages = target.GetProfileGraphPages("day");

      // Assert
      Assert.That(pages, Is.EquivalentTo(new[] { "p1", "p2", "" }));
    }

    [Test]
    public void GetProfileGraphs()
    {
      // Arrange
      var profileGraphDb1 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t1", Interval = "5-minutes", Rank = 1 };
      var profileGraphDb2 = new Db.ProfileGraph { Period = "day", Page = "", Title = "t2", Interval = "5-minutes", Rank = 1 };
      InsertProfileGraphs(profileGraphDb1, profileGraphDb2);
      var profileGraphSerieDb11 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb12 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA23, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb21 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb2.Id };
      InsertProfileGraphSeries(profileGraphSerieDb11, profileGraphSerieDb12, profileGraphSerieDb21);
      var target = CreateTarget();

      // Act
      var profileGraphs = target.GetProfileGraphs();

      // Assert
      Assert.That(profileGraphs.Count, Is.EqualTo(2));
      AssertProfileGraph(profileGraphDb1, new[] { profileGraphSerieDb11, profileGraphSerieDb12 } , profileGraphs.First());
      AssertProfileGraph(profileGraphDb2, new[] { profileGraphSerieDb21 }, profileGraphs.Last());
    }

    [Test]
    public void GetProfileGraphsPeriodPage()
    {
      // Arrange
      var profileGraphDb1 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t1", Interval = "5-minutes", Rank = 1 };
      var profileGraphDb2 = new Db.ProfileGraph { Period = "day", Page = "", Title = "t2", Interval = "5-minutes", Rank = 2 };
      var profileGraphDb3 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t3", Interval = "1-months", Rank = 3 };
      var profileGraphDb4 = new Db.ProfileGraph { Period = "month", Page = "", Title = "t4", Interval = "1-days", Rank = 4 };
      InsertProfileGraphs(profileGraphDb1, profileGraphDb2, profileGraphDb3, profileGraphDb4);
      var profileGraphSerieDb11 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb12 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA23, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb21 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb2.Id };
      var profileGraphSerieDb31 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb3.Id };
      var profileGraphSerieDb41 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb4.Id };
      InsertProfileGraphSeries(profileGraphSerieDb11, profileGraphSerieDb12, profileGraphSerieDb21, profileGraphSerieDb31, profileGraphSerieDb41);
      var target = CreateTarget();

      // Act
      var profileGraphs = target.GetProfileGraphs("day", "p1");

      // Assert
      Assert.That(profileGraphs.Count, Is.EqualTo(2));
      AssertProfileGraph(profileGraphDb1, new[] { profileGraphSerieDb11, profileGraphSerieDb12 }, profileGraphs.First());
      AssertProfileGraph(profileGraphDb3, new[] { profileGraphSerieDb31 }, profileGraphs.Last());
    }

    [Test]
    public void AddProfileGraph()
    {
      // Arrange
      var sn1 = new SeriesName("label", ObisCode.ElectrActiveEnergyA14Period);
      var sn2 = new SeriesName("label", ObisCode.ElectrActualPowerP14);
      var profileGraph = new ProfileGraph("day", "pPage", "pTitle", "5-minutes", 0, new [] {sn1, sn2});
      var target = CreateTarget();

      // Act
      target.AddProfileGraph(profileGraph);

      // Assert
      AssertProfileGraphExists(profileGraph);
    }

    [Test]
    public void AddProfileGraphExplicitRank()
    {
      // Arrange
      var sn1 = new SeriesName("label", ObisCode.ElectrActiveEnergyA14Period);
      var sn2 = new SeriesName("label", ObisCode.ElectrActualPowerP14);
      var profileGraph = new ProfileGraph("day", "pPage", "pTitle", "5-minutes", 100, new[] { sn1, sn2 });
      var target = CreateTarget();

      // Act
      target.AddProfileGraph(profileGraph);

      // Assert
      AssertProfileGraphExists(profileGraph);
    }

    [Test]
    public void AddProfileGraphDuplicate()
    {
      // Arrange
      var sn = new SeriesName("label", ObisCode.ElectrActiveEnergyA14Period);
      var profileGraph1 = new ProfileGraph("day", "pPage", "pTitle", "5-minutes", 1, new[] { sn });
      var profileGraph2 = new ProfileGraph("day", "pPage", "pTitle", "5-minutes", 2, new[] { sn });
      var target = CreateTarget();
      target.AddProfileGraph(profileGraph1);

      // Act & Assert
      Assert.That(() => target.AddProfileGraph(profileGraph2), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void AddProfileGraphDuplicateRank()
    {
      // Arrange
      var sn = new SeriesName("label", ObisCode.ElectrActiveEnergyA14Period);
      var profileGraph1 = new ProfileGraph("day", "pPage", "pTitle1", "5-minutes", 1, new[] { sn });
      var profileGraph2 = new ProfileGraph("day", "pPage", "pTitle2", "5-minutes", 1, new[] { sn });
      var target = CreateTarget();
      target.AddProfileGraph(profileGraph1);

      // Act & Assert
      Assert.That(() => target.AddProfileGraph(profileGraph2), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void DeleteProfileGraph()
    {
      // Arrange
      var profileGraph1 = new ProfileGraph("day", "pPage1", "pTitle1", "5-minutes", 1, new[] { new SeriesName("label1", ObisCode.ElectrActualPowerP14) });
      var profileGraph2 = new ProfileGraph("day", "", "pTitle2", "5-minutes", 2, new[] { new SeriesName("label2", ObisCode.ElectrActualPowerP14) });
      var target = CreateTarget();
      target.AddProfileGraph(profileGraph1);
      target.AddProfileGraph(profileGraph2);

      // Act
      target.DeleteProfileGraph(profileGraph1.Period, profileGraph1.Page, profileGraph1.Title);

      // Assert
      AssertProfileGraphExists(profileGraph1, not: true);
    }

    [Test]
    public void DeleteProfileGraphEmptyPage()
    {
      // Arrange
      var profileGraph = new ProfileGraph("day", "", "pTitle1", "5-minutes", 1, new[] { new SeriesName("label1", ObisCode.ElectrActualPowerP14) });
      var target = CreateTarget();
      target.AddProfileGraph(profileGraph);

      // Act
      target.DeleteProfileGraph(profileGraph.Period, profileGraph.Page, profileGraph.Title);

      // Assert
      AssertProfileGraphExists(profileGraph, not: true);
    }

    [Test]
    public void SwapProfileGraphRank()
    {
      // Arrange
      var profileGraph1 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t1", Interval = "5-minutes", Rank = 1 };
      var profileGraph2 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t2", Interval = "5-minutes", Rank = 2 };
      var profileGraph3 = new Db.ProfileGraph { Period = "day", Page = "p2", Title = "t1", Interval = "5-minutes", Rank = 3 };
      var profileGraph4 = new Db.ProfileGraph { Period = "month", Page = "p1", Title = "t1", Interval = "1-days", Rank = 3 };
      InsertProfileGraphs(profileGraph1, profileGraph2, profileGraph3, profileGraph4);
      var target = CreateTarget();

      // Act
      target.SwapProfileGraphRank(profileGraph1.Period, profileGraph1.Page, profileGraph1.Title, profileGraph2.Title);

      // Assert
      AssertProfileGraphExists(profileGraph1.Period, profileGraph1.Page, profileGraph1.Title, profileGraph2.Rank);
      AssertProfileGraphExists(profileGraph2.Period, profileGraph2.Page, profileGraph2.Title, profileGraph1.Rank);
      var profileGraphsDb = DbContext.QueryTransaction<Db.ProfileGraph>("", "SELECT * FROM ProfileGraph WHERE Rank=@Rank;", new { Rank = 3 });
      Assert.That(profileGraphsDb.Count, Is.EqualTo(2));
    }

    private static void AssertProfileGraph(Db.ProfileGraph dbProfileGraph, Db.ProfileGraphSerie[] dbProfileGraphSeries, ProfileGraph profileGraph)
    {
      Assert.That(profileGraph.Period, Is.EqualTo(dbProfileGraph.Period));
      Assert.That(profileGraph.Page, Is.EqualTo(dbProfileGraph.Page));
      Assert.That(profileGraph.Title, Is.EqualTo(dbProfileGraph.Title));
      Assert.That(profileGraph.Interval, Is.EqualTo(dbProfileGraph.Interval));
      Assert.That(profileGraph.SerieNames.Count, Is.EqualTo(dbProfileGraphSeries.Length));
      for (int ix = 0; ix < dbProfileGraphSeries.Length; ix++)
      {
        Assert.That(profileGraph.SerieNames[ix].Label, Is.EqualTo(dbProfileGraphSeries[ix].Label));
        Assert.That(profileGraph.SerieNames[ix].ObisCode, Is.EqualTo((ObisCode)dbProfileGraphSeries[ix].ObisCode));
      }
    }

    private void AssertProfileGraphExists(string period, string page, string title, long rank, bool not = false)
    {
      var profileGraphsDb = DbContext.QueryTransaction<Db.ProfileGraph>("",
        "SELECT * FROM ProfileGraph WHERE Period=@period AND Page=@page AND Title=@title AND Rank=@rank;",
        new { period, page, title, rank });
      Assert.That(profileGraphsDb.Count, Is.EqualTo(not ? 0 : 1));
    }

    private void AssertProfileGraphExists(ProfileGraph profileGraph, bool not = false)
    {
      var sql = "SELECT * FROM ProfileGraph WHERE Period=@Period AND Page=@Page AND Title=@Title AND Interval=@Interval AND Rank{0}@Rank;";
      var rankOp = "=";
      if (profileGraph.Rank == 0)
      {
        rankOp = ">";
      }
      sql = string.Format(CultureInfo.InvariantCulture, sql, rankOp);
      var profileGraphsDb = DbContext.QueryTransaction<Db.ProfileGraph>("", sql, profileGraph);
      Assert.That(profileGraphsDb.Count, Is.EqualTo(not ? 0 : 1));

      if (!not)
      {
        var profileGraphDb = profileGraphsDb.First();
        var profileGraphSeriesDb = DbContext.QueryTransaction<Db.ProfileGraphSerie>("", 
          "SELECT * FROM ProfileGraphSerie WHERE ProfileGraphId=@Id;", new { profileGraphDb.Id });
        Assert.That(profileGraph.SerieNames.Count, Is.EqualTo(profileGraphSeriesDb.Count));
        foreach (var serieName in profileGraph.SerieNames)
        {
          var dbProfileGraphserie = DbContext.QueryTransaction<Db.ProfileGraphSerie>("",
            "SELECT * FROM ProfileGraphSerie WHERE Label=@Label AND ObisCode=@ObisCode;", new { serieName.Label, Obiscode = (long)serieName.ObisCode });
          Assert.That(dbProfileGraphserie.Count, Is.EqualTo(1));
        }
      }
    }

    private ProfileGraphRepository CreateTarget()
    {
      return new ProfileGraphRepository(DbContext);
    }

    private void InsertProfileGraph(Db.ProfileGraph profileGraph)
    {
      InsertProfileGraphs(new Db.ProfileGraph[] { profileGraph });
    }

    private void InsertProfileGraphs(params Db.ProfileGraph[] profileGraphs)
    {
      foreach (var profileGraph in profileGraphs)
      {
        var id = DbContext.QueryTransaction<long>("",
          "INSERT INTO ProfileGraph (Period,Page,Title,Interval,Rank) VALUES (@Period,@Page,@Title,@Interval,@Rank); SELECT last_insert_rowid();",
          profileGraph).First();
        profileGraph.Id = id;
      }
    }

    private void InsertProfileGraphSerie(Db.ProfileGraphSerie profileGraphSerie)
    {
      InsertProfileGraphSeries(new Db.ProfileGraphSerie[] { profileGraphSerie });
    }

    private void InsertProfileGraphSeries(params Db.ProfileGraphSerie[] profileGraphSeries)
    {
      DbContext.ExecuteTransaction("",
        "INSERT INTO ProfileGraphSerie (Label,ObisCode,ProfileGraphId) VALUES (@Label,@ObisCode,@ProfileGraphId);",
        profileGraphSeries);
    }

  }
}
