using System.Collections.Generic;

namespace PowerView.Model.Repository
{
  public interface ILiveReadingRepository
  {
    void Add(IList<Reading> liveReadings);
  }
}
