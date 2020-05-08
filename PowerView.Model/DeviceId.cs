using System;

namespace PowerView.Model
{
  public static class DeviceId
  {
    public static bool Equals(string deviceId1, string deviceId2)
    {
      return string.Equals(deviceId1, deviceId2, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
