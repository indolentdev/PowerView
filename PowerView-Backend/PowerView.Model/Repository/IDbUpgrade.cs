
namespace PowerView.Model.Repository
{
  public interface IDbUpgrade
  {
    bool IsNeeded();
    void ApplyUpdates();
  }
}