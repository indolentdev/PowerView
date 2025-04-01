using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.DisconnectControl
{
    internal class DisconnectCalculator : IDisconnectCalculator
    {
        private readonly IDisconnectRuleRepository disconnectRuleRepository;

        public DisconnectCalculator(IDisconnectRuleRepository disconnectRuleRepository)
        {
            ArgumentNullException.ThrowIfNull(disconnectRuleRepository);

            this.disconnectRuleRepository = disconnectRuleRepository;
        }

        public void SynchronizeAndCalculate(DateTime time, IDisconnectCache disconnectCache, IList<Reading> liveReadings)
        {
            var disconnectRules = disconnectRuleRepository.GetDisconnectRules();
            disconnectCache.SynchronizeRules(disconnectRules.OfType<IDisconnectRule>().ToList());

            disconnectCache.Add(liveReadings);
            disconnectCache.Calculate(time);
        }

    }
}
