using System;
using System.Collections.Generic;
using PowerView.Model;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
  internal class Hub : IHub
  {
    private readonly IServiceProvider serviceProvider;
    private readonly IReadingPiper piper;
    private readonly IMeterEventCoordinator meterEventCoordinator;
    private readonly IMqttPublisherFactory mqttPublisherFactory;
    private readonly IDisconnectControlFactory disconnectControlFactory;
    private readonly IHealthCheck healthCheck;
    private readonly IEventQueue eventQueue;

    public Hub(IServiceProvider serviceProvider, IEventQueue eventQueue, IReadingPiper piper, IMeterEventCoordinator meterEventCoordinator, 
      IMqttPublisherFactory mqttPublisherFactory, IDisconnectControlFactory disconnectControlFactory, 
      IHealthCheck healthCheck)
    {
      this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
      this.eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
      this.piper = piper ?? throw new ArgumentNullException(nameof(piper));
      this.meterEventCoordinator = meterEventCoordinator ?? throw new ArgumentNullException(nameof(meterEventCoordinator));
      this.mqttPublisherFactory = mqttPublisherFactory ?? throw new ArgumentNullException(nameof(mqttPublisherFactory));
      this.disconnectControlFactory = disconnectControlFactory ?? throw new ArgumentNullException(nameof(disconnectControlFactory));
      this.healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
    }

    #region IHub implementation

    public void Signal(IList<Reading> liveReadings)
    {
      eventQueue.Enqueue(InScope((ss) => mqttPublisherFactory.Publish(ss, liveReadings)));
      eventQueue.Enqueue(InScope((ss) => disconnectControlFactory.Process(ss, liveReadings)));

      var utcNow = DateTime.UtcNow;
      eventQueue.Enqueue(InScope((ss) => healthCheck.DailyCheck(ss, utcNow)));
      eventQueue.Enqueue(InScope((ss) => piper.PipeLiveReadings(ss, utcNow)));
      eventQueue.Enqueue(InScope((ss) => piper.PipeDayReadings(ss, utcNow)));
      eventQueue.Enqueue(InScope((ss) => piper.PipeMonthReadings(ss, utcNow)));
      eventQueue.Enqueue(InScope((ss) => meterEventCoordinator.DetectAndNotify(ss, utcNow)));
    }

    #endregion

    private Action InScope(Action<IServiceScope> action)
    {
      return () => 
      {
        using var scope = serviceProvider.CreateScope();
        action(scope);
      };
    }

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

