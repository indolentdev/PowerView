using System;
using PowerView.Service;

namespace PowerView.ProcessStopper
{
  internal class WindowsProcessStopper : IProcessStopper
  {
    private readonly IExitSignalProvider exitSignalProvider;

    public WindowsProcessStopper(IExitSignalProvider exitSignalProvider)
    {
      if (exitSignalProvider == null) throw new ArgumentNullException("exitSignalProvider");

      this.exitSignalProvider = exitSignalProvider;
    }

    public void WireUp()
    {
      Console.WriteLine("PowerView is running. Press any key to exit");
      Console.Read();
      exitSignalProvider.FireExitEvent();
    }
  }
}

