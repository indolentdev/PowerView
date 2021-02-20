using System;
using System.Collections.Generic;

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
      if (normalizedTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("normalizedTimestamp", "Must be UTC");

      this.timeRegisterValue = timeRegisterValue;
      this.normalizedTimestamp = normalizedTimestamp;
    }

    public NormalizedDurationRegisterValue SubtractValue(NormalizedTimeRegisterValue baseValue)
    {
      var substractedValue = timeRegisterValue.UnitValue - baseValue.TimeRegisterValue.UnitValue;
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
          dValue = (maxValue - baseValue.TimeRegisterValue.UnitValue.Value) + TimeRegisterValue.UnitValue.Value;
        }
        else
        {
          var msg = string.Format("A calculation of a subtracted value resulted in a negative result. Minuend:{0}, Subtrahend:{1}",
            this, baseValue);
          throw new DataMisalignedException(msg);
        }
      }

      return new NormalizedDurationRegisterValue(baseValue.TimeRegisterValue.Timestamp, TimeRegisterValue.Timestamp,
        baseValue.NormalizedTimestamp, NormalizedTimestamp, new UnitValue(dValue, substractedValue.Unit), TimeRegisterValue.DeviceId);
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

    public bool Equals(NormalizedTimeRegisterValue value)
    {
      return value != null &&
             EqualityComparer<TimeRegisterValue>.Default.Equals(timeRegisterValue, value.timeRegisterValue) &&
             normalizedTimestamp == value.normalizedTimestamp;
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

