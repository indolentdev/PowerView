using System;
using System.Linq;
using NUnit.Framework;
using Dapper;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging.Abstractions;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DbUpgradeTest : DbTestFixture
  {
    [Test]
    public void IsNeededAndApplyUpdatesOnCleanDatabase()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var updateNeeded = target.IsNeeded();
      target.ApplyUpdates();

      // Assert
      Assert.That(updateNeeded, Is.True);
      Assert.That(DbContext.QueryTransaction<int>("SELECT Count(*) FROM Version;").FirstOrDefault(), Is.GreaterThan(1));
      Assert.That(GetTableCount(), Is.GreaterThan(1));
    }

    [Test]
    public void IsNeededOnHigherVersionDatabase()
    {
      // Arrange
      CreateVersionTableAndInsertVersion(long.MaxValue - 1);
      var target = CreateTarget();

      // Act
      var updateNeeded = target.IsNeeded();

      // Assert
      Assert.That(updateNeeded, Is.False);
      Assert.That(DbContext.QueryTransaction<int>("SELECT Count(*) FROM Version;").FirstOrDefault(), Is.EqualTo(1));
      Assert.That(GetTableCount(), Is.EqualTo(1));
    }

    [Test]
    public void IsNeededOnPreviouslyBrokenApplyUpdates()
    {
      // Arrange
      CreateVersionTableAndInsertVersion(long.MaxValue);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.IsNeeded(), Throws.TypeOf<InvalidOperationException>());
    }

    private DbUpgrade CreateTarget()
    {
      return new DbUpgrade(new NullLogger<DbUpgrade>(), DbContext);
    }

    private void CreateVersionTableAndInsertVersion(long version)
    {
      Connection.Execute("CREATE TABLE Version (Id INTEGER PRIMARY KEY, Number INTEGER NOT NULL, Timestamp DATETIME NOT NULL);");
      DbContext.ExecuteTransaction("INSERT INTO Version (Number, Timestamp) VALUES (@Number, @Timestamp);", 
        new { Number = version, Timestamp = DateTime.UtcNow });
    }

    private long GetTableCount()
    {
      var tableCountResult = Connection.Query("SELECT COUNT(*) AS TableCount FROM sqlite_master WHERE type='table';");
      return tableCountResult.First().TableCount;
    }
  }
}
