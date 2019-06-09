using PowerView.Model;

namespace PowerView.Service.Modules
{
  public interface IReadingAccepter
  {
    void Accept(LiveReading[] liveReadings);
  }
}
