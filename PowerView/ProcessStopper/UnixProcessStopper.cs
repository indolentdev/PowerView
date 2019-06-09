using System;
using log4net;
using PowerView.Service;

namespace PowerView.ProcessStopper
{
  internal class UnixProcessStopper : IProcessStopper
  {
    private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IExitSignalProvider exitSignalProvider;

    public UnixProcessStopper(IExitSignalProvider exitSignalProvider)
    {
      if (exitSignalProvider == null) throw new ArgumentNullException("exitSignalProvider");

      this.exitSignalProvider = exitSignalProvider;
    }

    public void WireUp()
    {
      log.Info("Run StopPowerView.sh to exit. (kill -SIGTERM " + System.Diagnostics.Process.GetCurrentProcess().Id + ")");
      using (var sigTerm = new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGTERM))
      {
        using (var sigInt = new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGINT))
        {
          var sigs = new [] { sigTerm, sigInt };
          // block until a SIGINT or SIGTERM signal is generated.
          var sigIx = Mono.Unix.UnixSignal.WaitAny(sigs);
          if (sigIx >= 0 && sigIx < sigs.Length)
          {
            log.DebugFormat("Received signal {0}.", sigs[sigIx].Signum);
            exitSignalProvider.FireExitEvent();
          }
        }
      }
    }
  }
}

