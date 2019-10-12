using System;
using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.EventHub
{
  internal class Hub : IHub
  {
    private readonly IReadingPiper piper;
    private readonly IMeterEventCoordinator meterEventCoordinator;
    private readonly ITracker tracker;
    private readonly IMqttPublisherFactory mqttPublisherFactory;
    private readonly IDisconnectControlFactory disconnectControlFactory;
    private readonly IHealthCheck healthCheck;
    private readonly EventQueue eventQueue;

    public Hub(IReadingPiper piper, IMeterEventCoordinator meterEventCoordinator, ITracker tracker, 
      IMqttPublisherFactory mqttPublisherFactory, IDisconnectControlFactory disconnectControlFactory, IHealthCheck healthCheck)
    {
      if (piper == null) throw new ArgumentNullException("piper");
      if (meterEventCoordinator == null) throw new ArgumentNullException("meterEventCoordinator");
      if (tracker == null) throw new ArgumentNullException("tracker");
      if (mqttPublisherFactory == null) throw new ArgumentNullException("mqttPublisherFactory");
      if (disconnectControlFactory == null) throw new ArgumentNullException("disconnectControlFactory");
      if (healthCheck == null) throw new ArgumentNullException("healthCheck");

      this.piper = piper;
      this.meterEventCoordinator = meterEventCoordinator;
      this.tracker = tracker;
      this.mqttPublisherFactory = mqttPublisherFactory;
      this.disconnectControlFactory = disconnectControlFactory;
      this.healthCheck = healthCheck;
      eventQueue = new EventQueue();
    }

    #region IHub implementation

    public void Signal(IList<LiveReading> liveReadings)
    {
      eventQueue.Enqueue(() => mqttPublisherFactory.Publish(liveReadings));
      eventQueue.Enqueue(() => disconnectControlFactory.Process(liveReadings));

      var now = DateTime.Now; // Hmm.. this actually depends on the host box having the correct time zone setup... :/
      var utcNow = DateTime.UtcNow;
      eventQueue.Enqueue(() => healthCheck.DailyCheck(utcNow));
      eventQueue.Enqueue(() => piper.PipeLiveReadings(now));
      eventQueue.Enqueue(() => piper.PipeDayReadings(now));
      eventQueue.Enqueue(() => piper.PipeMonthReadings(now));
      eventQueue.Enqueue(() => meterEventCoordinator.DetectAndNotify(now));
      eventQueue.Enqueue(() => tracker.Track(now));
    }

    #endregion

    #region IDisposable implementation
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Hub() 
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

