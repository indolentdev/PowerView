using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.Controllers;

public interface IReadingAccepter
{
    void Accept(IList<Reading> liveReadings);
}
