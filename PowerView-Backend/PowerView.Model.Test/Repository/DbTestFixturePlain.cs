using Microsoft.Data.Sqlite;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    public abstract class DbTestFixturePlain
    {
        protected SqliteConnection Connection { get; private set; }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DapperConfig.Configure();
        }

        [SetUp]
        public virtual void SetUp()
        {
            Connection = DbContextFactory.GetConnection(DbContextFactory.GetConnectionStringBuilder(":memory:"));
        }

        [TearDown]
        public virtual void TearDown()
        {
            Connection?.Dispose();
        }

    }
}