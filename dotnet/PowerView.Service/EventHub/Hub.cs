using System;
using System.Collections.Generic;
using PowerView.Model;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
  internal class Hub : IHub
  {
    private readonly IReadingPiper piper;
    private readonly IMeterEventCoordinator meterEventCoordinator;
    private readonly IMqttPublisherFactory mqttPublisherFactory;
    private readonly IDisconnectControlFactory disconnectControlFactory;
    private readonly IHealthCheck healthCheck;
    private readonly EventQueue eventQueue;

    public Hub(IReadingPiper piper, IMeterEventCoordinator meterEventCoordinator, 
      IMqttPublisherFactory mqttPublisherFactory, IDisconnectControlFactory disconnectControlFactory, 
      IHealthCheck healthCheck, EventQueue eventQueue)
    {
      this.piper = piper ?? throw new ArgumentNullException(nameof(piper));
      this.meterEventCoordinator = meterEventCoordinator ?? throw new ArgumentNullException(nameof(meterEventCoordinator));
      this.mqttPublisherFactory = mqttPublisherFactory ?? throw new ArgumentNullException(nameof(mqttPublisherFactory));
      this.disconnectControlFactory = disconnectControlFactory ?? throw new ArgumentNullException(nameof(disconnectControlFactory));
      this.healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
      eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
    }

    #region IHub implementation

    public void Signal(IList<LiveReading> liveReadings)
    {
      eventQueue.Enqueue(() => mqttPublisherFactory.Publish(liveReadings));
      eventQueue.Enqueue(() => disconnectControlFactory.Process(liveReadings));

      var utcNow = DateTime.UtcNow;
      eventQueue.Enqueue(() => healthCheck.DailyCheck(utcNow));
      eventQueue.Enqueue(() => piper.PipeLiveReadings(utcNow));
      eventQueue.Enqueue(() => piper.PipeDayReadings(utcNow));
      eventQueue.Enqueue(() => piper.PipeMonthReadings(utcNow));
      eventQueue.Enqueue(() => meterEventCoordinator.DetectAndNotify(utcNow));
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

