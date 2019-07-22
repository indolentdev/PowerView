using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PowerView.Model;
using log4net;

namespace PowerView.Service.DisconnectControl
{
  internal class DisconnectWarden : IDisconnectWarden, IDisconnectControlCache
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IFactory factory;
    private readonly EventQueue eventQueue;
    private readonly IDisconnectCache disconnectCache;
    private volatile IDictionary<ISeriesName, bool> statusReadCopy;

    public DisconnectWarden(IFactory factory)
    {
      if (factory == null) throw new ArgumentNullException("factory");

      this.factory = factory;
      eventQueue = new EventQueue();
      disconnectCache = new DisconnectCache();
      statusReadCopy = new Dictionary<ISeriesName, bool>(0);
    }

    public IDictionary<ISeriesName, bool> GetOutputStatus(string label)
    { // Purposely do not use the event queue.
      var statusReadCopyLocal = statusReadCopy; // locking free multithreading
      var filtered = statusReadCopyLocal
        .Where(x => string.Equals(x.Key.Label, label, StringComparison.InvariantCultureIgnoreCase))
        .ToDictionary(x => x.Key, x => x.Value);
      return filtered;
    }

    public void Process(IList<LiveReading> liveReadings)
    {
      var time = DateTime.UtcNow;
      eventQueue.Enqueue(() => ProcessInner(time, liveReadings));
    }

    private void ProcessInner(DateTime time, IList<LiveReading> liveReadings)
    {
      using (var calculator = factory.Create<IDisconnectCalculator>())
      {
        calculator.Value.SynchronizeAndCalculate(time, disconnectCache, liveReadings);
      }

      var newStatus = disconnectCache.GetStatus();
      var statusReadCopyLocal = statusReadCopy; // locking free multithreading
      var changes = newStatus.Except(statusReadCopyLocal).ToList();
      if (changes.Count > 0)
      {
        log.InfoFormat("Disconnect control changes:{0}", string.Join(", ", changes.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}-{1}:{2}", x.Key.Label, x.Key.ObisCode, x.Value))));
      }
      statusReadCopy = newStatus; // locking free multithreading
    }

    #region IDisposable implementation
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~DisconnectWarden()
    {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        eventQueue.Dispose();
      }
      // free native resources if there are any.
    }

    #endregion

  }
}
