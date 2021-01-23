using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.Modules
{
  public interface IReadingAccepter
  {
    void Accept(IList<LiveReading> liveReadings);
  }
}
