using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using System.Globalization;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class CostBreakdownRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new CostBreakdownRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetCostBreakdowns()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb1 = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      var costBreakdownEntryDb11 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      var costBreakdownEntryDb12 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(8), Name = "N12", StartTime = 3, EndTime = 21, Amount = 2.345678 };
      InsertCostBreakdown(costBreakdownDb1, costBreakdownEntryDb11, costBreakdownEntryDb12);
      var costBreakdownDb2 = new Db.CostBreakdown { Title = "t2", Currency = (int)Unit.Dkk, Vat = 25 };
      var costBreakdownEntryDb21 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(2), Name = "N21", StartTime = 0, EndTime = 23, Amount = 3.456789 };
      InsertCostBreakdown(costBreakdownDb2, costBreakdownEntryDb21);
      var target = CreateTarget();

      // Act
      var costBreakdowns = target.GetCostBreakdowns();

      // Assert
      Assert.That(costBreakdowns.Count, Is.EqualTo(2));
      AssertCostBreakdown(costBreakdownDb1, new[] { costBreakdownEntryDb11, costBreakdownEntryDb12 }, costBreakdowns.First());
      AssertCostBreakdown(costBreakdownDb2, new[] { costBreakdownEntryDb21 }, costBreakdowns.Last());
    }

    [Test]
    public void AddCostBreakdownThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddCostBreakdown(null), Throws.ArgumentNullException);
    }

    [Test]
    public void AddCostBreakdown()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var entry1 = new CostBreakdownEntry(dateTime, dateTime.AddDays(8), "E1", 2, 22, 1.234567);
      var entry2 = new CostBreakdownEntry(dateTime.AddDays(1), dateTime.AddDays(10), "E2", 3, 21, 2.345678);
      var costBreakdown = new CostBreakdown("Title", Unit.Dkk, 25, new[] { entry1, entry2 });
      var target = CreateTarget();

      // Act
      target.AddCostBreakdown(costBreakdown);

      // Assert
      AssertCostBreakdownExists(costBreakdown);
    }

    [Test]
    public void AddCostBreakdownDuplicate()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdown = new CostBreakdown("Title", Unit.Dkk, 25, Array.Empty<CostBreakdownEntry>());
      var target = CreateTarget();
      target.AddCostBreakdown(costBreakdown);

      // Act & Assert
      Assert.That(() => target.AddCostBreakdown(costBreakdown), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void AddCostBreakdownDuplicateEntry()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var entry = new CostBreakdownEntry(dateTime, dateTime.AddDays(8), "E1", 2, 22, 1.234567);
      var costBreakdown = new CostBreakdown("Title", Unit.Dkk, 25, new[] { entry, entry });
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddCostBreakdown(costBreakdown), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void DeleteCostBreakdownThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.DeleteCostBreakdown(null), Throws.ArgumentNullException);
    }

    [Test]
    public void DeleteCostBreakdown()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb1 = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      var costBreakdownEntryDb11 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      var costBreakdownEntryDb12 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(8), Name = "N12", StartTime = 3, EndTime = 21, Amount = 2.345678 };
      InsertCostBreakdown(costBreakdownDb1, costBreakdownEntryDb11, costBreakdownEntryDb12);
      var costBreakdownDb2 = new Db.CostBreakdown { Title = "t2", Currency = (int)Unit.Dkk, Vat = 25 };
      var costBreakdownEntryDb21 = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      InsertCostBreakdown(costBreakdownDb2, costBreakdownEntryDb21);
      var target = CreateTarget();

      // Act
      target.DeleteCostBreakdown(costBreakdownDb1.Title);

      // Assert
      AssertCostBreakdownExists(costBreakdownDb1.Title, costBreakdownDb1.Currency, costBreakdownDb1.Vat, not: true);
      AssertCostBreakdownExists(costBreakdownDb2.Title, costBreakdownDb2.Currency, costBreakdownDb2.Vat);
    }

    [Test]
    public void AddCostBreakdownEntryThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      const string title = "theTitle";
      var entry = new CostBreakdownEntry(dateTime, dateTime.AddDays(5), "e", 0, 23, 1.234567);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddCostBreakdownEntry(null, entry), Throws.ArgumentNullException);
      Assert.That(() => target.AddCostBreakdownEntry(title, null), Throws.ArgumentNullException);
    }

    [Test]
    public void AddCostBreakdownEntry()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      InsertCostBreakdown(costBreakdownDb);
      var entry = new CostBreakdownEntry(dateTime, dateTime.AddDays(5), "e", 0, 23, 1.234567);
      var target = CreateTarget();

      // Act
      target.AddCostBreakdownEntry(costBreakdownDb.Title, entry);

      // Assert
      AssertCostBreakdownEntryExists(costBreakdownDb.Id, entry);
    }

    [Test]
    public void AddCostBreakdownEntryCostBreakdownDoesNotExist()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var entry = new CostBreakdownEntry(dateTime, dateTime.AddDays(5), "e", 0, 23, 1.234567);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddCostBreakdownEntry("does not exist", entry), Throws.TypeOf<DataStoreException>());
    }

    [Test]
    public void AddCostBreakdownEntryDuplicate()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      InsertCostBreakdown(costBreakdownDb, new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(5), Name = "e", StartTime = 1, EndTime = 22, Amount = 1.23 });
      var entry = new CostBreakdownEntry(dateTime, dateTime.AddDays(5), "e", 0, 23, 1.234567);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddCostBreakdownEntry(costBreakdownDb.Title, entry), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void UpdateCostBreakdownEntryThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      const string title = "theTitle";
      const string name = "theName";
      var target = CreateTarget();
      var costBreakdownEntry = new CostBreakdownEntry(dateTime.AddDays(-1), dateTime.AddDays(6), "Name", 4, 19, 9.876543);

      // Act & Assert
      Assert.That(() => target.UpdateCostBreakdownEntry(null, dateTime, dateTime, name, costBreakdownEntry), Throws.ArgumentNullException);
      Assert.That(() => target.UpdateCostBreakdownEntry(title, dateTime, dateTime, null, costBreakdownEntry), Throws.ArgumentNullException);
      Assert.That(() => target.UpdateCostBreakdownEntry(title, dateTime, dateTime, name, null), Throws.ArgumentNullException);
    }

    [Test]
    public void UpdateCostBreakdownEntry()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      var costBreakdownEntryDb = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      InsertCostBreakdown(costBreakdownDb, costBreakdownEntryDb);
      var target = CreateTarget();
      var costBreakdownEntry = new CostBreakdownEntry(dateTime.AddDays(-1), dateTime.AddDays(6), "OtherName", 4, 19, 9.876543);

      // Act
      target.UpdateCostBreakdownEntry(costBreakdownDb.Title, costBreakdownEntryDb.FromDate, costBreakdownEntryDb.ToDate, costBreakdownEntryDb.Name, costBreakdownEntry);

      // Assert
      Assert.That(DbContext.QueryTransaction<int>("SELECT Count(*) FROM CostBreakdownEntry WHERE CostBreakdownId=@Id;", costBreakdownDb).First(), Is.EqualTo(1));
      AssertCostBreakdownEntryExists(costBreakdownDb.Id, costBreakdownEntry);
    }

    [Test]
    public void UpdateCostBreakdownEntryWhenCostBreakdownDoesNotExistThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownEntryDb = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      var target = CreateTarget();
      var costBreakdownEntry = new CostBreakdownEntry(dateTime.AddDays(-1), dateTime.AddDays(6), "OtherName", 4, 19, 9.876543);

      // Act & Assert
      Assert.That(() => target.UpdateCostBreakdownEntry("DoesNotExist", costBreakdownEntryDb.FromDate, costBreakdownEntryDb.ToDate, costBreakdownEntryDb.Name, costBreakdownEntry), Throws.TypeOf<DataStoreException>());
    }

    [Test]
    public void UpdateCostBreakdownEntryWhenCostBreakdownEntryDoesNotExistThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      InsertCostBreakdown(costBreakdownDb);
      var target = CreateTarget();
      var costBreakdownEntry = new CostBreakdownEntry(dateTime.AddDays(-1), dateTime.AddDays(6), "OtherName", 4, 19, 9.876543);

      // Act & Assert
      Assert.That(() => target.UpdateCostBreakdownEntry(costBreakdownDb.Title, dateTime, dateTime, "DoesNotExist", costBreakdownEntry), Throws.TypeOf<DataStoreException>());
    }

    [Test]
    public void DeleteCostBreakdownEntryThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      const string title = "theTitle";
      const string name = "theName";
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.DeleteCostBreakdownEntry(null, dateTime, dateTime, name), Throws.ArgumentNullException);
      Assert.That(() => target.DeleteCostBreakdownEntry(title, dateTime, dateTime, null), Throws.ArgumentNullException);
    }

    [Test]
    public void DeleteCostBreakdownEntry()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var costBreakdownDb = new Db.CostBreakdown { Title = "t1", Currency = (int)Unit.Eur, Vat = 10 };
      var costBreakdownEntryDb = new Db.CostBreakdownEntry { FromDate = dateTime, ToDate = dateTime.AddDays(4), Name = "N11", StartTime = 2, EndTime = 20, Amount = 1.234567 };
      InsertCostBreakdown(costBreakdownDb, costBreakdownEntryDb);
      var target = CreateTarget();

      // Act
      target.DeleteCostBreakdownEntry(costBreakdownDb.Title, costBreakdownEntryDb.FromDate, costBreakdownEntryDb.ToDate, costBreakdownEntryDb.Name);

      // Assert
      Assert.That(DbContext.QueryTransaction<int>("SELECT Count(*) FROM CostBreakdownEntry WHERE CostBreakdownId=@Id;", costBreakdownDb).First(), Is.EqualTo(0));
    }

    [Test]
    public void DeleteCostBreakdownEntryWhenCostBreakdownDoesNotExistThrows()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.DeleteCostBreakdownEntry("DoesNotExist", dateTime, dateTime, "Name"), Throws.TypeOf<DataStoreException>());
    }

    private static void AssertCostBreakdown(Db.CostBreakdown dbCostBreakdown, Db.CostBreakdownEntry[] dbCostBreakdownEntry, CostBreakdown costBreakdown)
    {
      Assert.That(costBreakdown.Title, Is.EqualTo(dbCostBreakdown.Title));
      Assert.That(costBreakdown.Currency, Is.EqualTo((Unit)dbCostBreakdown.Currency));
      Assert.That(costBreakdown.Vat, Is.EqualTo(dbCostBreakdown.Vat));
      Assert.That(costBreakdown.Entries.Count, Is.EqualTo(dbCostBreakdownEntry.Length));
      for (int ix = 0; ix < dbCostBreakdownEntry.Length; ix++)
      {
        Assert.That(costBreakdown.Entries[ix].FromDate, Is.EqualTo(dbCostBreakdownEntry[ix].FromDate.ToDateTime()));
        Assert.That(costBreakdown.Entries[ix].ToDate, Is.EqualTo(dbCostBreakdownEntry[ix].ToDate.ToDateTime()));
        Assert.That(costBreakdown.Entries[ix].Name, Is.EqualTo(dbCostBreakdownEntry[ix].Name));
        Assert.That(costBreakdown.Entries[ix].StartTime, Is.EqualTo(dbCostBreakdownEntry[ix].StartTime));
        Assert.That(costBreakdown.Entries[ix].EndTime, Is.EqualTo(dbCostBreakdownEntry[ix].EndTime));
        Assert.That(costBreakdown.Entries[ix].Amount, Is.EqualTo(dbCostBreakdownEntry[ix].Amount));
      }
    }

    private void AssertCostBreakdownExists(string title, int currency, int vat, bool not = false)
    {
      var costBreakdownDb = DbContext.QueryTransaction<Db.CostBreakdown>(
        "SELECT * FROM CostBreakdown WHERE Title=@title AND Currency=@currency AND Vat=@vat;",
        new { title, currency, vat });
      Assert.That(costBreakdownDb.Count, Is.EqualTo(not ? 0 : 1));
    }

    private void AssertCostBreakdownExists(CostBreakdown costBreakdown, bool not = false)
    {
      var sql = "SELECT * FROM CostBreakdown WHERE Title=@Title AND Currency=@Currency AND Vat=@Vat;";
      var costBreakdownsDb = DbContext.QueryTransaction<Db.ProfileGraph>(sql, costBreakdown);
      Assert.That(costBreakdownsDb.Count, Is.EqualTo(not ? 0 : 1));

      if (!not)
      {
        var costBreakdownDb = costBreakdownsDb.First();
        var costBreakdownEntriesDb = DbContext.QueryTransaction<Db.CostBreakdownEntry>(
          "SELECT * FROM CostBreakdownEntry WHERE CostBreakdownId=@Id;", new { costBreakdownDb.Id });
        Assert.That(costBreakdown.Entries.Count, Is.EqualTo(costBreakdownEntriesDb.Count));
        foreach (var costBreakdownEntry in costBreakdown.Entries)
        {
          var dbCostBreakdownEntry = DbContext.QueryTransaction<Db.CostBreakdownEntry>(
            "SELECT * FROM CostBreakdownEntry WHERE CostBreakdownId=@CostBreakdownId AND FromDate=@FromDate AND ToDate=@ToDate AND Name=@Name AND StartTime=@StartTime AND EndTime=@EndTime AND Amount=@Amount;", 
            new Db.CostBreakdownEntry { CostBreakdownId=costBreakdownDb.Id, FromDate = (UnixTime)costBreakdownEntry.FromDate, ToDate = (UnixTime)costBreakdownEntry.ToDate,
              Name = costBreakdownEntry.Name, StartTime = costBreakdownEntry.StartTime, EndTime = costBreakdownEntry.EndTime, Amount = costBreakdownEntry.Amount });
          Assert.That(dbCostBreakdownEntry.Count, Is.EqualTo(1));
        }
      }
    }

    private void AssertCostBreakdownEntryExists(long id, CostBreakdownEntry costBreakdownEntry, bool not = false)
    {
          var dbCostBreakdownEntry = DbContext.QueryTransaction<Db.CostBreakdownEntry>(
            "SELECT * FROM CostBreakdownEntry WHERE CostBreakdownId=@CostBreakdownId AND FromDate=@FromDate AND ToDate=@ToDate AND Name=@Name AND StartTime=@StartTime AND EndTime=@EndTime AND Amount=@Amount;", 
            new Db.CostBreakdownEntry { CostBreakdownId=id, FromDate = (UnixTime)costBreakdownEntry.FromDate, ToDate = (UnixTime)costBreakdownEntry.ToDate,
              Name = costBreakdownEntry.Name, StartTime = costBreakdownEntry.StartTime, EndTime = costBreakdownEntry.EndTime, Amount = costBreakdownEntry.Amount });
          Assert.That(dbCostBreakdownEntry.Count, Is.EqualTo(not ? 0 : 1));
    }

    private CostBreakdownRepository CreateTarget()
    {
      return new CostBreakdownRepository(DbContext);
    }

    private void InsertCostBreakdown(Db.CostBreakdown costBreakdown, params Db.CostBreakdownEntry[] costBreakdownEntries)
    {
      var id = DbContext.QueryTransaction<long>(
        "INSERT INTO CostBreakdown (Title,Currency,Vat) VALUES (@Title,@Currency,@Vat); SELECT last_insert_rowid();",
        costBreakdown).First();
      costBreakdown.Id = id;

      foreach (var entry in costBreakdownEntries)
      {
        entry.CostBreakdownId = id;
      }

      DbContext.ExecuteTransaction(
        "INSERT INTO CostBreakdownEntry (CostBreakdownId,FromDate,ToDate,Name,StartTime,EndTime,Amount) VALUES (@CostBreakdownId,@FromDate,@ToDate,@Name,@StartTime,@EndTime,@Amount);", costBreakdownEntries);
    }



  }
}
