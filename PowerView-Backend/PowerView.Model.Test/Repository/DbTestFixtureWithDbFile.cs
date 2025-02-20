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

        public string DbName { get; protected set; }
        public bool DeleteDbFiles { get; protected set; } = true;
        private DbContextFactory dbContextFactory;

        internal DbContext DbContext { get; private set; }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DapperConfig.Configure();
            dbContextFactory = new DbContextFactory(new DatabaseOptions { Name = DbName });
            DbContext = (DbContext)dbContextFactory.CreateContext();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            DbContext?.Dispose();

            if (DeleteDbFiles)
            {
                if (File.Exists(DbName))
                {
                    File.Delete(DbName);
                }
                if (File.Exists(DbName + "-journal"))
                {
                    File.Delete(DbName + "-journal");
                }
            }
        }

    }
}
