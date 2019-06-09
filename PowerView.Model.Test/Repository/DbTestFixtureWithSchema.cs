using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  public class DbTestFixtureWithSchema : DbTestFixture
  {
    public override void SetUp()
    {
      base.SetUp();

      new DbUpgrade(DbContext).ApplyUpdates();
    }
  }
}

