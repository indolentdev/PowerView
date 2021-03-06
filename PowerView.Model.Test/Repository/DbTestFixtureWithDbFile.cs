using System;
using System.IO;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  public class DbTestFixtureWithDbFile
  {
    public DbTestFixtureWithDbFile()
    {
      DbName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid() + ".sqlite3";
    }

    public string DbName { get; private set; }
    private DbContextFactory dbContextFactory;

    internal DbContext DbContext { get; private set; }

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      dbContextFactory = new DbContextFactory(DbName);
      DbContext = (DbContext)dbContextFactory.CreateContext();
    }

    [OneTimeTearDown]
    public void TestFixtureTearDown()
    {
      if (DbContext != null)
      {
        DbContext.Dispose();
      }

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
