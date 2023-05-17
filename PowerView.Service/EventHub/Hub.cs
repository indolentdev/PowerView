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
        private readonly IEnergiDataServiceImport energiDataServiceImport;
        private readonly IEventQueue eventQueue;

        public Hub(IServiceProvider serviceProvider, IEventQueue eventQueue, IReadingPiper piper, IMeterEventCoordinator meterEventCoordinator,
          IMqttPublisherFactory mqttPublisherFactory, IDisconnectControlFactory disconnectControlFactory,
          IHealthCheck healthCheck, IEnergiDataServiceImport energiDataServiceImport)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
            this.piper = piper ?? throw new ArgumentNullException(nameof(piper));
            this.meterEventCoordinator = meterEventCoordinator ?? throw new ArgumentNullException(nameof(meterEventCoordinator));
            this.mqttPublisherFactory = mqttPublisherFactory ?? throw new ArgumentNullException(nameof(mqttPublisherFactory));
            this.disconnectControlFactory = disconnectControlFactory ?? throw new ArgumentNullException(nameof(disconnectControlFactory));
            this.healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
            this.energiDataServiceImport = energiDataServiceImport ?? throw new ArgumentNullException(nameof(energiDataServiceImport));
        }

        #region IHub implementation

        public void Signal(IList<Reading> liveReadings)
        {
            eventQueue.Enqueue(InScopeAction((ss) => mqttPublisherFactory.Publish(ss, liveReadings)));
            eventQueue.Enqueue(InScopeAction((ss) => disconnectControlFactory.Process(ss, liveReadings)));

            var utcNow = DateTime.UtcNow;
            eventQueue.Enqueue(InScopeAction((ss) => healthCheck.DailyCheck(ss, utcNow)));
            eventQueue.Enqueue(InScopeAction((ss) => piper.PipeLiveReadings(ss, utcNow)));
            eventQueue.Enqueue(InScopeAction((ss) => piper.PipeDayReadings(ss, utcNow)));
            eventQueue.Enqueue(InScopeAction((ss) => piper.PipeMonthReadings(ss, utcNow)));
            eventQueue.Enqueue(InScopeAction((ss) => meterEventCoordinator.DetectAndNotify(ss, utcNow)));
            eventQueue.Enqueue(InScopeFunc((ss) => energiDataServiceImport.Import(ss, utcNow)));
        }

        #endregion

        private Action InScopeAction(Action<IServiceScope> action)
        {
            return () =>
            {
                using var scope = serviceProvider.CreateScope();
                action(scope);
            };
        }

        private Action InScopeFunc(Func<IServiceScope, Task> func)
        {
            return () =>
            {
                using var scope = serviceProvider.CreateScope();
                func(scope).GetAwaiter().GetResult(); // TODO: Have (I)EventQueue support Func<Task>.
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

