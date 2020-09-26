
namespace PowerView.Configuration
{
  internal interface IPowerViewConfiguration
  {
    ServiceSection GetServiceSection();
    DatabaseSection GetDatabaseSection();
  }
}
