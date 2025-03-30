using System;
using System.Collections.Generic;
using System.Globalization;

namespace PowerView.Model
{
    public class TimeRegisterValue : IEquatable<TimeRegisterValue>, IRegisterValue
    {
        public const string DummyDeviceId = "0";

        private readonly string deviceId;
        private readonly DateTime timestamp;
        private readonly UnitValue unitValue;

        public string DeviceId { get { return deviceId; } }
        public DateTime Timestamp { get { return timestamp; } }
        public UnitValue UnitValue { get { return unitValue; } }

        public DateTime OrderProperty { get { return Timestamp; } }

        public TimeRegisterValue(string deviceId, DateTime timestamp, int value, short scale, Unit unit)
          : this(deviceId, timestamp, new UnitValue(value, scale, unit))
        {
        }

        internal TimeRegisterValue(string deviceId, DateTime timestamp, double value, Unit unit)
          : this(deviceId, timestamp, new UnitValue(value, unit))
        {
        }

        internal TimeRegisterValue(string deviceId, DateTime timestamp, UnitValue unitValue)
        {
            ArgCheck.ThrowIfNotUtc(timestamp);

            this.deviceId = deviceId;
            this.timestamp = timestamp;
            this.unitValue = unitValue;
        }

        public NormalizedTimeRegisterValue Normalize(Func<DateTime, DateTime> timeDivider)
        {
            ArgumentNullException.ThrowIfNull(timeDivider);

            return new NormalizedTimeRegisterValue(this, timeDivider(Timestamp));
        }

        public TimeRegisterValue SubtractValue(TimeRegisterValue baseValue)
        {
            var substractedValue = unitValue - baseValue.unitValue;
            var dValue = substractedValue.Value;

            if (!DeviceIdEquals(baseValue))
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "A calculation of a subtracted value was not possible. The values originate from different devices (device ids). Minuend:{0}, Subtrahend:{1}",
                  this, baseValue);
                throw new DataMisalignedException(msg);
            }

            if (dValue < 0)
            {
                var maxValue = GetMaxValue(baseValue);
                if (dValue * -1 < maxValue * 0.05) // Assume register quirk (e.g. meter reboot without proper data continuation/data restore)
                {
                    dValue = 0;
                }
                else if (dValue * -1 > maxValue * 0.75) // Assume register wrap
                {
                    dValue = (maxValue - baseValue.unitValue.Value) + unitValue.Value;
                }
                else
                {
                    var msg = string.Format(CultureInfo.InvariantCulture, "A calculation of a subtracted value resulted in a negative result. Minuend:{0}, Subtrahend:{1}",
                      this, baseValue);
                    throw new DataMisalignedException(msg);
                }
            }

            return new TimeRegisterValue(deviceId, timestamp, dValue, substractedValue.Unit);
        }

        private static double GetMaxValue(TimeRegisterValue timeRegisterValue)
        {
            var longValue = Convert.ToInt64(timeRegisterValue.unitValue.Value);
            var pow = longValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
            return Math.Pow(10, pow);
        }

        public bool DeviceIdEquals(TimeRegisterValue timeRegisterValue)
        {
            return Model.DeviceId.Equals(DeviceId, timeRegisterValue.DeviceId);
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[deviceId={0}, timestamp={1}, unitValue={2}]",
              deviceId, timestamp.ToString("o"), unitValue);
        }

        public override bool Equals(object obj)
        {
            var value = obj as TimeRegisterValue;
            return Equals(value);
        }

        public bool Equals(TimeRegisterValue other)
        {
            return other != null &&
                   DeviceIdEquals(other) &&
                   timestamp == other.timestamp &&
                   unitValue.Equals(other.unitValue);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1356502293;
                hashCode = hashCode * -1521134295 + deviceId != null ? deviceId.ToLowerInvariant().GetHashCode() : 0;
                hashCode = hashCode * -1521134295 + timestamp.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<UnitValue>.Default.GetHashCode(unitValue);
                return hashCode;
            }
        }

        public static bool operator ==(TimeRegisterValue value1, TimeRegisterValue value2)
        {
            return EqualityComparer<TimeRegisterValue>.Default.Equals(value1, value2);
        }

        public static bool operator !=(TimeRegisterValue value1, TimeRegisterValue value2)
        {
            return !(value1 == value2);
        }

    }
}

