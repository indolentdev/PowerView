
namespace PowerView.Model.Repository
{
    public interface ICrudeDataRepository
    {
        WithCount<ICollection<CrudeDataValue>> GetCrudeData(string label, DateTime from, int skip = 0, int take = 3000);

        ICollection<CrudeDataValue> GetCrudeDataBy(string label, DateTime timestamp);

        void DeleteCrudeData(string label, DateTime timestamp, ObisCode obisCode);

        IList<MissingDate> GetMissingDays(string label);
    }
}
