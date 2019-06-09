using System;
using System.Linq;
using NUnit.Framework;
using Dapper;
using DapperExtensions;
using PowerView.Model.Repository;

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
      Assert.That(Connection.Count<DbUpgrade.Version>(null), Is.GreaterThan(1));
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
      Assert.That(Connection.Count<DbUpgrade.Version>(null), Is.EqualTo(1));
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
      return new DbUpgrade(DbContext);
    }

    private void CreateVersionTableAndInsertVersion(long version)
    {
      Connection.Execute("CREATE TABLE Version (Id INTEGER PRIMARY KEY, Number INTEGER NOT NULL, Timestamp DATETIME NOT NULL);");
      Connection.Insert(new DbUpgrade.Version { Number = version });
    }

    private long GetTableCount()
    {
      var tableCountResult = Connection.Query("SELECT COUNT(*) AS TableCount FROM sqlite_master WHERE type='table';");
      return tableCountResult.First().TableCount;
    }
  }
}
