using System.Collections.Generic;

namespace PowerView.Model.Repository
{
    public interface IMeterEventRepository
    {
        ICollection<MeterEvent> GetLatestMeterEventsByLabel();

        WithCount<ICollection<MeterEvent>> GetMeterEvents(int skip = 0, int take = 50);

        void AddMeterEvents(IEnumerable<MeterEvent> newMeterEvents);

        long? GetMaxFlaggedMeterEventId();
    }
}
