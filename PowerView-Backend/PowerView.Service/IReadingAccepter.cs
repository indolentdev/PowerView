using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service;

public interface IReadingAccepter
{
    void Accept(IList<Reading> liveReadings);
}
