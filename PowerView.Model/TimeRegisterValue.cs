using System;

namespace PowerView.Model
{
  public struct TimeRegisterValue : IEquatable<TimeRegisterValue>, ISeries
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
      if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("timestamp", "Must be UTC");

      this.deviceId = deviceId;
      this.timestamp = timestamp;
      this.unitValue = unitValue;
    }

    public NormalizedTimeRegisterValue Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      return new NormalizedTimeRegisterValue(this, timeDivider(Timestamp));
    }

    public TimeRegisterValue SubtractValue(TimeRegisterValue baseValue)
    {
      var substractedValue = unitValue - baseValue.unitValue;
      var dValue = substractedValue.Value;

      if (!DeviceIdEquals(baseValue))
      {
        var msg = string.Format("A calculation of a subtracted value was not possible. The values originate from different devices (device ids). Minuend:{0}, Subtrahend:{1}",
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
          var msg = string.Format("A calculation of a subtracted value resulted in a negative result. Minuend:{0}, Subtrahend:{1}",
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
      if (obj == null) return false;
      if (obj.GetType() != typeof(TimeRegisterValue)) return false;
      var other = (TimeRegisterValue)obj;
      return Equals(other);
    }

    public bool Equals(TimeRegisterValue other)
    {
      return DeviceIdEquals(other) && timestamp == other.timestamp && unitValue == other.UnitValue;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (deviceId != null ? deviceId.ToLowerInvariant().GetHashCode() : 0) ^ timestamp.GetHashCode() ^ unitValue.GetHashCode();
      }
    }

    public static bool operator ==(TimeRegisterValue x, TimeRegisterValue y) 
    {
      return x.Equals(y);
    }

    public static bool operator !=(TimeRegisterValue x, TimeRegisterValue y) 
    {
      return !(x == y);
    }

  }
}

