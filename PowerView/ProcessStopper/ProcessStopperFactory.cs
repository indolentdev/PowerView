using System;
using PowerView.Service;

namespace PowerView.ProcessStopper
{
  internal static class ProcessStopperFactory
  {
    public static IProcessStopper Create(string osVersion, IExitSignalProvider exitSignalProvider)
    {
      if (osVersion.ToLower().Contains("unix"))
      {
        return new UnixProcessStopper(exitSignalProvider);
      }
      else
      {
        return new WindowsProcessStopper(exitSignalProvider);
      }
    }
  }
}

