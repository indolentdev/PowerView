using System;
using System.Collections.Generic;
using System.Globalization;

namespace PowerView.Model
{
    public class NormalizedTimeRegisterValue : IEquatable<NormalizedTimeRegisterValue>, IRegisterValue
    {
        private readonly TimeRegisterValue timeRegisterValue;
        private readonly DateTime normalizedTimestamp;

        public TimeRegisterValue TimeRegisterValue { get { return timeRegisterValue; } }
        public DateTime NormalizedTimestamp { get { return normalizedTimestamp; } }

        public DateTime OrderProperty { get { return TimeRegisterValue.Timestamp; } }

        internal NormalizedTimeRegisterValue(TimeRegisterValue timeRegisterValue, DateTime normalizedTimestamp)
        {
            ArgCheck.ThrowIfNotUtc(normalizedTimestamp);

            this.timeRegisterValue = timeRegisterValue;
            this.normalizedTimestamp = normalizedTimestamp;
        }

        public NormalizedDurationRegisterValue SubtractAccommodateWrap(NormalizedTimeRegisterValue baseValue)
        {
            if (!DeviceIdEquals(baseValue))
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "A calculation of a subtracted value was not possible. The values originate from different devices (device ids). Minuend:{0}, Subtrahend:{1}",
                  this, baseValue);
                throw new DataMisalignedException(msg);
            }

            return Subtract(baseValue, SubtractNegativeValueHanlding.QuirkAndWrap);
        }

        public NormalizedDurationRegisterValue SubtractNotNegative(NormalizedTimeRegisterValue baseValue)
        {
            return Subtract(baseValue, SubtractNegativeValueHanlding.NotNegative);
        }

        private enum SubtractNegativeValueHanlding
        {
            None,
            QuirkAndWrap,
            NotNegative
        }

        private NormalizedDurationRegisterValue Subtract(NormalizedTimeRegisterValue baseValue, SubtractNegativeValueHanlding handling)
        {
            var substractedValue = timeRegisterValue.UnitValue - baseValue.TimeRegisterValue.UnitValue;
            var dValue = substractedValue.Value;

            if (dValue < 0)
            {
                if (handling == SubtractNegativeValueHanlding.QuirkAndWrap)
                {
                    dValue = HandleQuirkAndWrap(baseValue, dValue);
                }
                else if (handling == SubtractNegativeValueHanlding.NotNegative)
                {
                    dValue = 0;
                }
            }

            return new NormalizedDurationRegisterValue(baseValue.TimeRegisterValue.Timestamp, TimeRegisterValue.Timestamp,
              baseValue.NormalizedTimestamp, NormalizedTimestamp, new UnitValue(dValue, substractedValue.Unit), TimeRegisterValue.DeviceId, baseValue.TimeRegisterValue.DeviceId);
        }

        private double HandleQuirkAndWrap(NormalizedTimeRegisterValue baseValue, double dValue)
        {
            var maxValue = GetMaxValue(baseValue);
            if (dValue * -1 < maxValue * 0.05) // Assume register quirk (e.g. meter reboot without proper data continuation/data restore)
            {
                dValue = 0;
            }
            else if (dValue * -1 > maxValue * 0.75) // Assume register wrap
            {
                dValue = (maxValue - baseValue.TimeRegisterValue.UnitValue.Value) + TimeRegisterValue.UnitValue.Value;
            }
            else
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "A calculation of a subtracted value resulted in a negative result. Minuend:{0}, Subtrahend:{1}",
                  this, baseValue);
                throw new DataMisalignedException(msg);
            }
            return dValue;
        }

        private static double GetMaxValue(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
        {
            var longValue = Convert.ToInt64(normalizedTimeRegisterValue.TimeRegisterValue.UnitValue.Value);
            var pow = longValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
            return Math.Pow(10, pow);
        }

        public bool DeviceIdEquals(NormalizedTimeRegisterValue normalizedTimeRegisterValue)
        {
            return TimeRegisterValue.DeviceIdEquals(normalizedTimeRegisterValue.TimeRegisterValue);
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[timeRegisterValue={0}, normalizedTimestamp={1}]",
              timeRegisterValue, normalizedTimestamp.ToString("o"));
        }

        public override bool Equals(object obj)
        {
            var value = obj as NormalizedTimeRegisterValue;
            return Equals(value);
        }

        public bool Equals(NormalizedTimeRegisterValue other)
        {
            return other != null &&
                   EqualityComparer<TimeRegisterValue>.Default.Equals(timeRegisterValue, other.timeRegisterValue) &&
                   normalizedTimestamp == other.normalizedTimestamp;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1321262762;
                hashCode = hashCode * -1521134295 + EqualityComparer<TimeRegisterValue>.Default.GetHashCode(timeRegisterValue);
                hashCode = hashCode * -1521134295 + normalizedTimestamp.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(NormalizedTimeRegisterValue value1, NormalizedTimeRegisterValue value2)
        {
            return EqualityComparer<NormalizedTimeRegisterValue>.Default.Equals(value1, value2);
        }

        public static bool operator !=(NormalizedTimeRegisterValue value1, NormalizedTimeRegisterValue value2)
        {
            return !(value1 == value2);
        }

    }
}

