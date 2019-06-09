using System;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Modules
{
  public class ReadingAccepter : IReadingAccepter
  {
    private readonly ILiveReadingRepository liveReadingRepository;
    private readonly IHub hub;

    public ReadingAccepter(ILiveReadingRepository liveReadingRepository, IHub hub)
    {
      if (liveReadingRepository == null) throw new ArgumentNullException("liveReadingRepository");
      if (hub == null) throw new ArgumentNullException("streamSignal");

      this.liveReadingRepository = liveReadingRepository;
      this.hub = hub;
    }

    public void Accept(LiveReading[] liveReadings)
    {
      liveReadingRepository.Add(liveReadings);
      hub.Signal(liveReadings);
    }

  }
}
