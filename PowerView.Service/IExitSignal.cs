using System;
namespace PowerView.Service
{
  public interface IExitSignal
  {
    event EventHandler Exit;
  }
}
