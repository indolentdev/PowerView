using System;

namespace PowerView.Model
{
  public struct CoarseTimeRegisterValue : IEquatable<CoarseTimeRegisterValue>
  {
    private readonly DateTime coarseTimestamp;
    private readonly TimeRegisterValue timeRegisterValue;

    public DateTime CoarseTimestamp { get { return coarseTimestamp; } }
    public TimeRegisterValue TimeRegisterValue { get { return timeRegisterValue; } }

    public CoarseTimeRegisterValue(DateTime coarseTimestamp, TimeRegisterValue timeRegisterValue)
    {
      if ( coarseTimestamp.Kind != DateTimeKind.Utc ) throw new ArgumentOutOfRangeException("coarseTimestamp", "Must be UTC");

      this.coarseTimestamp = coarseTimestamp;
      this.timeRegisterValue = timeRegisterValue;
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[coarseTimestamp={0}, timeRegisterValue={1}]", 
        coarseTimestamp.ToString("o"), timeRegisterValue);
    }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != typeof(CoarseTimeRegisterValue)) return false;
      var other = (CoarseTimeRegisterValue)obj;
      return Equals(other);
    }

    public bool Equals(CoarseTimeRegisterValue other)
    {
      return coarseTimestamp == other.coarseTimestamp && timeRegisterValue == other.timeRegisterValue;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return coarseTimestamp.GetHashCode() ^ timeRegisterValue.GetHashCode();
      }
    }

    public static bool operator ==(CoarseTimeRegisterValue x, CoarseTimeRegisterValue y) 
    {
      return x.Equals(y);
    }

    public static bool operator !=(CoarseTimeRegisterValue x, CoarseTimeRegisterValue y) 
    {
      return !(x == y);
    }

  }
}

