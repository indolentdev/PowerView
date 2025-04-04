﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PowerView.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace PowerView.Service.DisconnectControl
{
    internal class DisconnectWarden : IDisconnectWarden, IDisconnectControlCache
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IEventQueue eventQueue;
        private readonly DisconnectCache disconnectCache;
        private volatile IDictionary<ISeriesName, bool> statusReadCopy;

        public DisconnectWarden(ILogger<DisconnectWarden> logger, IServiceProvider serviceProvider, IEventQueue eventQueue)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
            disconnectCache = new DisconnectCache();
            statusReadCopy = new Dictionary<ISeriesName, bool>(0);
        }

        public IDictionary<ISeriesName, bool> GetOutputStatus(string label)
        { // Purposely do not use the event queue.
            var statusReadCopyLocal = statusReadCopy; // locking free multithreading
#pragma warning disable CA1309 // Use ordinal string comparison
            var filtered = statusReadCopyLocal
              .Where(x => string.Equals(x.Key.Label, label, StringComparison.InvariantCultureIgnoreCase))
              .ToDictionary(x => x.Key, x => x.Value);
#pragma warning restore CA1309 // Use ordinal string comparison
            return filtered;
        }

        public void Process(IList<Reading> liveReadings)
        {
            var time = DateTime.UtcNow;
            eventQueue.Enqueue(() => ProcessInner(time, liveReadings));
        }

        private void ProcessInner(DateTime time, IList<Reading> liveReadings)
        {
            var calculator = serviceProvider.GetRequiredService<IDisconnectCalculator>();
            calculator.SynchronizeAndCalculate(time, disconnectCache, liveReadings);

            var newStatus = disconnectCache.GetStatus();
            var statusReadCopyLocal = statusReadCopy; // locking free multithreading
            var changes = newStatus.Except(statusReadCopyLocal).ToList();
            if (changes.Count > 0)
            {
                logger.LogInformation("Disconnect control changes:{Changes}", string.Join(", ", changes.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}-{1}:{2}", x.Key.Label, x.Key.ObisCode, x.Value))));
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
