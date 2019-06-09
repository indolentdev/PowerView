using System.Data;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  public class DbTestFixture
  {
    private DbContextFactory dbContextFactory;

    internal DbContext DbContext { get; private set; }
    protected IDbConnection Connection { get { return DbContext.Connection; } }

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      dbContextFactory = new DbContextFactory(":memory:");
    }

    [SetUp]
    public virtual void SetUp()
    {
      DbContext = (DbContext)dbContextFactory.CreateContext();
    }

    [TearDown]
    public void TearDown()
    {
      DbContext.Dispose();
    }

  }
}