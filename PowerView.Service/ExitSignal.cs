using System;
namespace PowerView.Service
{
  public class ExitSignal : IExitSignal, IExitSignalProvider
  {
    public event EventHandler Exit;

    public void FireExitEvent()
    {
      var exitEvent = Exit;
      if (exitEvent != null)
      {
        exitEvent(this, EventArgs.Empty);
      }
    }

  }
}
