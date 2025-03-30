using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
    public class DisconnectCacheItem : IDisconnectCacheItem
    {
        private readonly IDisconnectRule rule;
        private readonly List<TimeRegisterValue> registerValues;
        private bool connected;

        public DisconnectCacheItem(IDisconnectRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            this.rule = rule;
            registerValues = new List<TimeRegisterValue>();
            connected = false;
        }

        public IDisconnectRule Rule { get { return rule; } }
        public bool Connected { get { return connected; } }
        public int Count { get { return registerValues.Count; } }

        public void Add(IEnumerable<TimeRegisterValue> values)
        {
            foreach (var val in values)
            {
                if (!registerValues.Contains(val))
                {
                    registerValues.Add(val);
                }
            }
        }

        public void Calculate(DateTime time)
        {
            ArgCheck.ThrowIfNotUtc(time);

            CleanupRegisterValues(time);

            if (!CheckRegisterValues())
            {
                connected = false;
                return;
            }

            var avgValue = registerValues.Select(x => x.UnitValue.Value).Sum() / registerValues.Count;
            var refValue = connected ? Rule.ConnectToDisconnectValue : Rule.DisconnectToConnectValue;
            connected = avgValue > refValue;
        }

        private void CleanupRegisterValues(DateTime time)
        {
            var minTimestamp = time - Rule.Duration;
            var obsoleteValues = registerValues.Where(rv => rv.Timestamp < minTimestamp || rv.UnitValue.Unit != Rule.Unit).ToList();
            foreach (var obsoleteValue in obsoleteValues)
            {
                registerValues.Remove(obsoleteValue);
            }
        }

        private bool CheckRegisterValues()
        {
            if (registerValues.Count == 0)
            {
                return false;
            }

            var minValueTimestamp = registerValues.Select(x => x.Timestamp).Min();
            var maxValueTimestamp = registerValues.Select(x => x.Timestamp).Max();
            var minDuration = TimeSpan.FromSeconds(Rule.Duration.TotalSeconds * 0.65);
            var valueDuration = maxValueTimestamp - minValueTimestamp;
            if (valueDuration < minDuration)
            {
                return false;
            }

            return true;
        }

    }
}
