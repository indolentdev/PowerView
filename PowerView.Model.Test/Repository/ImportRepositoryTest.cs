using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using System.Globalization;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class ImportRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new ImportRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetImportsEmpty()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var imports = target.GetImports();

      // Assert
      Assert.That(imports, Is.Empty);
    }

    [Test]
    public void GetImports()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var importDb1 = new Db.Import { Label = "l1", Channel = "CH1", Currency = (int)Unit.Eur, FromTimestamp = dateTime, Enabled = false };
      var importDb2 = new Db.Import { Label = "l2", Channel = "CH2", Currency = (int)Unit.Dkk, FromTimestamp = dateTime.AddHours(2), CurrentTimestamp = dateTime.AddDays(2), Enabled = true };
      InsertImports(importDb1, importDb2);
      var target = CreateTarget();

      // Act
      var imports = target.GetImports();

      // Assert
      Assert.That(imports.Count, Is.EqualTo(2));
      AssertImport(importDb1, imports.First());
      AssertImport(importDb2, imports.Last());
    }

    [Test]
    public void AddImportThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddImport(null), Throws.ArgumentNullException);
    }

    [Test]
    public void AddImport()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var import = new Import("lbl", "CH1", Unit.Eur, dateTime, null, true);
      var target = CreateTarget();

      // Act
      target.AddImport(import);

      // Assert
      AssertImportExists(import);
    }

    [Test]
    public void AddImportDuplicate()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var import = new Import("lbl", "CH1", Unit.Eur, dateTime, null, true);
      var target = CreateTarget();
      target.AddImport(import);

      // Act & Assert
      Assert.That(() => target.AddImport(import), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }

    [Test]
    public void DeleteImportThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.DeleteImport(null), Throws.ArgumentNullException);
    }

    [Test]
    public void DeleteImport()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var importDb1 = new Db.Import { Label = "l1", Channel = "CH1", Currency = (int)Unit.Eur, FromTimestamp = dateTime, Enabled = false };
      var importDb2 = new Db.Import { Label = "l2", Channel = "CH2", Currency = (int)Unit.Dkk, FromTimestamp = dateTime.AddHours(2), CurrentTimestamp = dateTime.AddDays(2), Enabled = true };
      InsertImports(importDb1, importDb2);
      var target = CreateTarget();

      // Act
      target.DeleteImport(importDb1.Label);

      // Assert
      AssertImportExists(importDb1.Label, importDb1.Channel, importDb1.Currency, importDb1.FromTimestamp, importDb1.Enabled, not: true);
      AssertImportExists(importDb2.Label, importDb2.Channel, importDb2.Currency, importDb2.FromTimestamp, importDb2.Enabled);
    }

    [Test]
    public void SetEnabledThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.SetEnabled(null, true), Throws.ArgumentNullException);
    }

    [Test]
    public void SetEnabled()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var importDb = new Db.Import { Label = "l1", Channel = "CH1", Currency = (int)Unit.Eur, FromTimestamp = dateTime, Enabled = false };
      InsertImports(importDb);
      var target = CreateTarget();

      // Act
      target.SetEnabled(importDb.Label, true);

      // Assert
      AssertImportExists(importDb.Label, importDb.Channel, importDb.Currency, importDb.FromTimestamp, true);
    }

    [Test]
    public void SetEnabledDoesNotExist()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      target.SetEnabled("whatever", true);

      // Assert
      // does not throw
    }

    [Test]
    public void SetCurrentTimestampThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.SetCurrentTimestamp(null, DateTime.UtcNow), Throws.ArgumentNullException);
      Assert.That(() => target.SetCurrentTimestamp("lbl", DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void SetCurrentTimestamp()
    {
      // Arrange
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);
      var importDb = new Db.Import { Label = "l1", Channel = "CH1", Currency = (int)Unit.Eur, FromTimestamp = dateTime, Enabled = false };
      InsertImports(importDb);
      var target = CreateTarget();
      var dateTimeCurrent = dateTime.AddHours(3);

      // Act
      target.SetCurrentTimestamp(importDb.Label, dateTimeCurrent);

      // Assert
      var importDbs = DbContext.QueryTransaction<Db.Import>("SELECT * FROM Import;");
      Assert.That(importDbs.Count, Is.EqualTo(1));
      Assert.That(importDbs[0].CurrentTimestamp, Is.EqualTo((UnixTime)dateTimeCurrent));
    }

    [Test]
    public void SetCurrentTimestampDoesNotExist()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2023, 9, 3, 15, 54, 28, DateTimeKind.Utc);

      // Act
      target.SetCurrentTimestamp("whatever", dateTime);

      // Assert
      // does not throw
    }

    private static void AssertImport(Db.Import dbImport, Import import)
    {
      Assert.That(import.Label, Is.EqualTo(dbImport.Label));
      Assert.That(import.Channel, Is.EqualTo(dbImport.Channel));
      Assert.That(import.Currency, Is.EqualTo((Unit)dbImport.Currency));
      Assert.That(import.FromTimestamp, Is.EqualTo((DateTime)dbImport.FromTimestamp));
      Assert.That(import.CurrentTimestamp, Is.EqualTo((DateTime?)dbImport.CurrentTimestamp));
      Assert.That(import.Enabled, Is.EqualTo(dbImport.Enabled));
    }

    private void AssertImportExists(string label, string channel, int currency, UnixTime fromTimestamp, bool enabled, bool not = false)
    {
      var importDb = DbContext.QueryTransaction<Db.Import>(
        "SELECT * FROM Import WHERE Label=@label AND Channel=@channel AND Currency=@currency AND FromTimestamp=@fromTimestamp AND Enabled=@enabled;",
        new { label, channel, currency, fromTimestamp, enabled });
      Assert.That(importDb.Count, Is.EqualTo(not ? 0 : 1));
    }

    private void AssertImportExists(Import import, bool not = false)
    {
      AssertImportExists(import.Label, import.Channel, (int)import.Currency, import.FromTimestamp, import.Enabled);
    }

    private ImportRepository CreateTarget()
    {
      return new ImportRepository(DbContext);
    }

    private void InsertImports(params Db.Import[] imports)
    {
      DbContext.ExecuteTransaction(
        "INSERT INTO Import (Label,Channel,Currency,FromTimestamp,CurrentTimestamp,Enabled) VALUES (@Label,@Channel,@Currency,@FromTimestamp,@CurrentTimestamp,@Enabled);", imports);
    }

  }
}
