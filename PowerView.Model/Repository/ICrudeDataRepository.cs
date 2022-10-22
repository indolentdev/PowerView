
namespace PowerView.Model.Repository
{
  public interface ICrudeDataRepository
  {
    WithCount<ICollection<CrudeDataValue>> GetCrudeData(string label, DateTime from, int skip = 0, int take = 3000);

    IList<MissingDate> GetMissingDays(string label);
  }
}
