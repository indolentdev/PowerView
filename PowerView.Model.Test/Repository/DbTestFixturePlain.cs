using Microsoft.Data.Sqlite;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    public class DbTestFixturePlain
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
        public void TearDown()
        {
            Connection?.Dispose();
        }

    }
}