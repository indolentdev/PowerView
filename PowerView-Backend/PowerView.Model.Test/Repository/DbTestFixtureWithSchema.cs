using PowerView.Model.Repository;
using Microsoft.Extensions.Logging.Abstractions;

namespace PowerView.Model.Test.Repository
{
    public class DbTestFixtureWithSchema : DbTestFixture
    {
        public override void SetUp()
        {
            base.SetUp();

            new DbUpgrade(new NullLogger<DbUpgrade>(), DbContext).ApplyUpdates();
        }
    }
}

