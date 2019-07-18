using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

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
      DbContext.InsertTransaction("", (IEnumerable<Db.ProfileGraph>)new[] { pg1, pg2, pg3, pg4, pg5 });
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
      DbContext.InsertTransaction("", profileGraphDb1);
      var profileGraphSerieDb11 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb12 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA23, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb1 = new[] { profileGraphSerieDb11, profileGraphSerieDb12 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb1);
      var profileGraphDb2 = new Db.ProfileGraph { Period = "day", Page = "", Title = "t2", Interval = "5-minutes", Rank = 1 };
      DbContext.InsertTransaction("", profileGraphDb2);
      var profileGraphSerieDb21 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb2.Id };
      var profileGraphSerieDb2 = new[] { profileGraphSerieDb21 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb2);
      var target = CreateTarget();

      // Act
      var profileGraphs = target.GetProfileGraphs();

      // Assert
      Assert.That(profileGraphs.Count, Is.EqualTo(2));
      AssertProfileGraph(profileGraphDb1, profileGraphSerieDb1, profileGraphs.First());
      AssertProfileGraph(profileGraphDb2, profileGraphSerieDb2, profileGraphs.Last());
    }

    [Test]
    public void GetProfileGraphsPeriodPage()
    {
      // Arrange
      var profileGraphDb1 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t1", Interval = "5-minutes", Rank = 1 };
      DbContext.InsertTransaction("", profileGraphDb1);
      var profileGraphSerieDb11 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb12 = new Db.ProfileGraphSerie { Label = "l1", ObisCode = ObisCode.ElectrActiveEnergyA23, ProfileGraphId=profileGraphDb1.Id };
      var profileGraphSerieDb1 = new[] { profileGraphSerieDb11, profileGraphSerieDb12 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb1);
      var profileGraphDb2 = new Db.ProfileGraph { Period = "day", Page = "", Title = "t2", Interval = "5-minutes", Rank = 2 };
      DbContext.InsertTransaction("", profileGraphDb2);
      var profileGraphSerieDb21 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb2.Id };
      var profileGraphSerieDb2 = new[] { profileGraphSerieDb21 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb2);
      var profileGraphDb3 = new Db.ProfileGraph { Period = "day", Page = "p1", Title = "t3", Interval = "1-months", Rank = 3 };
      DbContext.InsertTransaction("", profileGraphDb3);
      var profileGraphSerieDb31 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb3.Id };
      var profileGraphSerieDb3 = new[] { profileGraphSerieDb31 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb3);
      var profileGraphDb4 = new Db.ProfileGraph { Period = "month", Page = "", Title = "t4", Interval = "1-days", Rank = 4 };
      DbContext.InsertTransaction("", profileGraphDb4);
      var profileGraphSerieDb41 = new Db.ProfileGraphSerie { Label = "l2", ObisCode = ObisCode.ElectrActiveEnergyA14, ProfileGraphId=profileGraphDb4.Id };
      var profileGraphSerieDb4 = new[] { profileGraphSerieDb41 };
      DbContext.InsertTransaction<Db.ProfileGraphSerie>("", profileGraphSerieDb4);
      var target = CreateTarget();

      // Act
      var profileGraphs = target.GetProfileGraphs("day", "p1");

      // Assert
      Assert.That(profileGraphs.Count, Is.EqualTo(2));
      AssertProfileGraph(profileGraphDb1, profileGraphSerieDb1, profileGraphs.First());
      AssertProfileGraph(profileGraphDb3, profileGraphSerieDb3, profileGraphs.Last());
    }

    [Test]
    public void AddProfileGraph()
    {
      // Arrange
      var sn1 = new SerieName("label", ObisCode.ElectrActiveEnergyA14Period);
      var sn2 = new SerieName("label", ObisCode.ElectrActualPowerP14);
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
      var sn1 = new SerieName("label", ObisCode.ElectrActiveEnergyA14Period);
      var sn2 = new SerieName("label", ObisCode.ElectrActualPowerP14);
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
      var sn = new SerieName("label", ObisCode.ElectrActiveEnergyA14Period);
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
      var sn = new SerieName("label", ObisCode.ElectrActiveEnergyA14Period);
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
      var profileGraph1 = new ProfileGraph("day", "pPage1", "pTitle1", "5-minutes", 1, new[] { new SerieName("label1", ObisCode.ElectrActualPowerP14) });
      var profileGraph2 = new ProfileGraph("day", "", "pTitle2", "5-minutes", 2, new[] { new SerieName("label2", ObisCode.ElectrActualPowerP14) });
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
      var profileGraph = new ProfileGraph("day", "", "pTitle1", "5-minutes", 1, new[] { new SerieName("label1", ObisCode.ElectrActualPowerP14) });
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
      DbContext.InsertTransaction("", (IEnumerable<Db.ProfileGraph>)new [] { profileGraph1, profileGraph2, profileGraph3, profileGraph4 });
      var target = CreateTarget();

      // Act
      target.SwapProfileGraphRank(profileGraph1.Period, profileGraph1.Page, profileGraph1.Title, profileGraph2.Title);

      // Assert
      AssertProfileGraphExists(profileGraph1.Period, profileGraph1.Page, profileGraph1.Title, profileGraph2.Rank);
      AssertProfileGraphExists(profileGraph2.Period, profileGraph2.Page, profileGraph2.Title, profileGraph1.Rank);
      var predicateRank = Predicates.Field<Db.ProfileGraph>(x => x.Rank, Operator.Eq, 3);
      var profileGraphsDb = DbContext.GetPage<Db.ProfileGraph>("", 0, 10, predicateRank);
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
      var predicatePeriod = Predicates.Field<Db.ProfileGraph>(x => x.Period, Operator.Eq, period);
      var predicatePage = Predicates.Field<Db.ProfileGraph>(x => x.Page, Operator.Eq, page);
      var predicateTitle = Predicates.Field<Db.ProfileGraph>(x => x.Title, Operator.Eq, title);
      var predicateRank = Predicates.Field<Db.ProfileGraph>(x => x.Rank, Operator.Eq, rank);
      var predicate = Predicates.Group(GroupOperator.And, predicateTitle, predicatePage, predicatePeriod, predicateRank);
      var profileGraphsDb = DbContext.GetPage<Db.ProfileGraph>("", 0, 10, predicate);
      Assert.That(profileGraphsDb.Count, Is.EqualTo(not ? 0 : 1));
    }

    private void AssertProfileGraphExists(ProfileGraph profileGraph, bool not = false)
    {
      var predicatePeriod = Predicates.Field<Db.ProfileGraph>(x => x.Period, Operator.Eq, profileGraph.Period);
      var predicatePage = Predicates.Field<Db.ProfileGraph>(x => x.Page, Operator.Eq, profileGraph.Page);
      var predicateTitle = Predicates.Field<Db.ProfileGraph>(x => x.Title, Operator.Eq, profileGraph.Title);
      var predicateInterval = Predicates.Field<Db.ProfileGraph>(x => x.Interval, Operator.Eq, profileGraph.Interval);
      var predicateRank = Predicates.Field<Db.ProfileGraph>(x => x.Rank, Operator.Eq, profileGraph.Rank);
      if (profileGraph.Rank == 0)
      {
        predicateRank = Predicates.Field<Db.ProfileGraph>(x => x.Rank, Operator.Gt, profileGraph.Rank);
      }
      var predicate = Predicates.Group(GroupOperator.And, predicateTitle, predicatePage, predicatePeriod, predicateInterval, predicateRank);
      var profileGraphsDb = DbContext.GetPage<Db.ProfileGraph>("", 0, 10, predicate);
      Assert.That(profileGraphsDb.Count, Is.EqualTo(not ? 0 : 1));
      if (!not)
      {
        var profileGraphDb = profileGraphsDb.First();
        var predicateSerieId = Predicates.Field<Db.ProfileGraphSerie>(x => x.ProfileGraphId, Operator.Eq, profileGraphDb.Id);
        var profileGraphSeriesDb = DbContext.GetPage<Db.ProfileGraphSerie>("", 0, 100, predicateSerieId);
        Assert.That(profileGraph.SerieNames.Count, Is.EqualTo(profileGraphSeriesDb.Count));
        foreach (var serieName in profileGraph.SerieNames)
        {
          var predicateLabel = Predicates.Field<Db.ProfileGraphSerie>(x => x.Label, Operator.Eq, serieName.Label);
          var predicateObisCode = Predicates.Field<Db.ProfileGraphSerie>(x => x.ObisCode, Operator.Eq, (long)serieName.ObisCode);
          var predicateSerie = Predicates.Group(GroupOperator.And, predicateSerieId, predicateLabel, predicateObisCode);
          Assert.That(DbContext.Connection.Count<Db.ProfileGraphSerie>(predicateSerie), Is.EqualTo(1));
        }
      }
    }

    private ProfileGraphRepository CreateTarget()
    {
      return new ProfileGraphRepository(DbContext);
    }

  }
}
