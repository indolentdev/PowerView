using System;

namespace PowerView.Model
{
    public class LeakMeterEventAmplification : IMeterEventAmplification
    {
        public LeakMeterEventAmplification(DateTime start, DateTime end, UnitValue unitValue)
        {
            Init(start, end, unitValue);
        }

        internal LeakMeterEventAmplification(Repository.IEntityDeserializer serializer)
        {
            var start = serializer.GetValue<DateTime>("Start");
            var end = serializer.GetValue<DateTime>("End");
            var value = serializer.GetValue<double>("UnitValue", "Value");
            var unitLong = serializer.GetValue<long>("UnitValue", "Unit");

            Init(start, end, new UnitValue(value, (Unit)unitLong));
        }

        private void Init(DateTime start, DateTime end, UnitValue unitValue)
        {
            ArgCheck.ThrowIfNotUtc(start);
            ArgCheck.ThrowIfNotUtc(end);

            Start = start;
            End = end;
            UnitValue = unitValue;
        }

        public string GetMeterEventType()
        {
            return "Leak";
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public UnitValue UnitValue { get; private set; }
    }
}
