using System;

namespace PowerView.Model
{
  public struct TimeRegisterValue : IEquatable<TimeRegisterValue>
  {
    public const string DummySerialNumber = "0";

    private readonly string serialNumber;
    private readonly DateTime timestamp;
    private readonly UnitValue unitValue;

    public string SerialNumber { get { return serialNumber; } }
    public DateTime Timestamp { get { return timestamp; } }
    public UnitValue UnitValue { get { return unitValue; } }

    public TimeRegisterValue(string serialNumber, DateTime timestamp, int value, short scale, Unit unit)
    {
      if ( timestamp.Kind != DateTimeKind.Utc ) throw new ArgumentOutOfRangeException("timestamp", "Must be UTC");

      this.serialNumber = serialNumber;
      this.timestamp = timestamp;
      unitValue = new UnitValue(value, scale, unit);
    }

    internal TimeRegisterValue(string serialNumber, DateTime timestamp, double value, Unit unit)
    {
      if ( timestamp.Kind != DateTimeKind.Utc ) throw new ArgumentOutOfRangeException("timestamp", "Must be UTC");

      this.serialNumber = serialNumber;
      this.timestamp = timestamp;
      unitValue = new UnitValue(value, unit);
    }

    public TimeRegisterValue SubtractValue(TimeRegisterValue baseValue)
    {
      var substractedValue = unitValue - baseValue.unitValue;
      var dValue = substractedValue.Value;

      if (!string.Equals(serialNumber, baseValue.serialNumber, StringComparison.InvariantCultureIgnoreCase))
      {
        var msg = string.Format("A calculation of a subtracted value was not possible. The values originate from different devices (serial numbers). Minuend:{0}, Subtrahend:{1}",
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

      return new TimeRegisterValue(serialNumber, timestamp, dValue, substractedValue.Unit);
    }

    private static double GetMaxValue(TimeRegisterValue timeRegisterValue)
    {
      var longValue = Convert.ToInt64(timeRegisterValue.unitValue.Value);
      var pow = longValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
      return Math.Pow(10, pow);
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[serialNumber={0}, timestamp={1}, unitValue={2}]", 
        serialNumber, timestamp.ToString("o"), unitValue);
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
      return string.Equals(serialNumber, other.serialNumber, StringComparison.InvariantCultureIgnoreCase) && timestamp == other.timestamp && unitValue == other.UnitValue;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (serialNumber != null ? serialNumber.ToLowerInvariant().GetHashCode() : 0) ^ timestamp.GetHashCode() ^ unitValue.GetHashCode();
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

