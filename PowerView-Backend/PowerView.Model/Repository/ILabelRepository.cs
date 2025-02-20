
namespace PowerView.Model.Repository
{
  public interface ILabelRepository
  {
    IList<string> GetLabelsByTimestamp();
  }
}

