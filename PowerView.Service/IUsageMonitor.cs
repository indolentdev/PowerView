﻿
namespace PowerView.Service
{
  public interface IUsageMonitor
  {
    void TrackDing(string sqliteVersion, string monoRuntimeVersion, string raspberryPiModelRevision);
  }
}
