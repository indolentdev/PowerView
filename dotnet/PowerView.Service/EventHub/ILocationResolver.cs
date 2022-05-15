
namespace PowerView.Service.EventHub
{
  /// <summary>
  /// Web based lookup of location/region info.
  /// Intended for application init.
  /// </summary>
  public interface ILocationResolver
  {
    void Resolve();
  }
}
