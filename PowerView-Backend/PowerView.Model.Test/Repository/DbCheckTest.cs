using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging.Abstractions;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DbCheckTest : DbTestFixtureWithDbFile
  {
    [Test]
    [Order(1)]
    public void CheckDatabaseOk()
    {
      // Arrange
      var target = CreateTarget(DbContext);

      // Act
      target.CheckDatabase();

      // Assert
      // Does not throw
    }

    [Test]
    [Order(2)]
    public void CheckDatabaseCorrupted()
    {
      // Arrange
      CreateTableAndInsertRows();
      DbContext.Dispose();
      var bytes = File.ReadAllBytes(DbName);
      var rnd = new Random();

      // triggering database corruption is not exact sience.. if it does not corrupt properly then give it a few more tries...
      for (var i=0; i < 25; i++)
      {
        // Take the first bytes from origin, then mangle a percentage of the remaining bytes...
        var corruptedBytes = bytes.Take(20000)
            .Concat(bytes.Skip(20000).Select(b => rnd.NextDouble() > 0.05 ? (byte?)b : 0xFF).Where(b => b != null).Select(b => b.Value))
            .ToArray();
        var corruptDbName = "Corrupted_" + DbName;
        File.WriteAllBytes(corruptDbName, corruptedBytes);

        DbContext dbContext = null;
        try
        {
          dbContext = (DbContext)new DbContextFactory(new DatabaseOptions { Name = corruptDbName }).CreateContext();
          var target = CreateTarget(dbContext);

          // Act && Assert
          try
          {
            target.CheckDatabase();
          }
          catch (DataStoreCorruptException)
          {
            return; // Corruption detected.. Test verified..
          }

          // Cleanup
        }
        finally
        {
          if (dbContext != null) dbContext.Dispose();

          if (File.Exists(corruptDbName))
          {
            File.Delete(corruptDbName);
          }
          if (File.Exists(corruptDbName + "-journal"))
          {
            File.Delete(corruptDbName + "-journal");
          }
        }

        // If we got to here then no of the attempts produced a corrupt database...
        Assert.Inconclusive("Did not succeed in corrupting the database....");
      }
    }

    private void CreateTableAndInsertRows()
    {
      DbContext.ExecuteTransaction("CREATE TABLE TheTable (Id INTEGER PRIMARY KEY, StringVal NVARCHAR(512) NOT NULL, IntVal INTEGER NOT NULL);" +
      	                           "CREATE UNIQUE INDEX IX ON TheTable (IntVal, StringVal, Id);" +
      	                           "CREATE UNIQUE INDEX IX2 ON TheTable (StringVal, IntVal, Id);");
      for (var i=0; i < 75; i++)
      {
        DbContext.ExecuteTransaction("INSERT INTO TheTable (StringVal, IntVal) VALUES (@StringVal, @IntVal);",
          new { StringVal = "This is the string value... " + string.Join("", Enumerable.Repeat(i + "-", 10)), IntVal = i + 1000000 });
      }
    }

    private DbCheck CreateTarget(DbContext dbContext)
    {
      return new DbCheck(new NullLogger<DbCheck>(), dbContext, new DatabaseCheckOptions { IntegrityCheckCommandTimeout = 30 });
    }

  }
}
