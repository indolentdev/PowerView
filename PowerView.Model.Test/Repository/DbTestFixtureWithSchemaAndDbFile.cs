using System;
using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Dapper;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  public class DbTestFixtureWithSchemaAndDbFile
  {
    public DbTestFixtureWithSchemaAndDbFile()
    {
      DbName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + ".sqlite3";
    }

    public string DbName { get; private set; }
    private DbContextFactory dbContextFactory;

    internal DbContext DbContext { get; private set; }
    protected IDbConnection Connection { get { return DbContext.Connection; } }

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      dbContextFactory = new DbContextFactory(DbName);
      DbContext = (DbContext)dbContextFactory.CreateContext();

      new DbUpgrade(DbContext).ApplyUpdates();
    }

    [SetUp]
    public void SetUp()
    {
      Connection.Execute("PRAGMA foreign_keys = OFF");

      var userTablesResult = Connection.Query("SELECT name AS Name FROM sqlite_master WHERE type='table';").ToArray();
      foreach (dynamic table in userTablesResult)
      {
        if (table.Name == "Version")
        {
          continue;
        }
        string deleteTableSql = "DELETE FROM " + table.Name;
        Connection.Execute(deleteTableSql);
      }

      Connection.Execute("PRAGMA foreign_keys = ON");
    }

    [OneTimeTearDown]
    public void TestFixtureTearDown()
    {
      DbContext.Dispose();

      if (File.Exists(DbName))
      {
        File.Delete(DbName);
      }
      if (File.Exists(DbName+"-journal"))
      {
        File.Delete(DbName+"-journal");
      }
    }

  }
}
