using System;

namespace PowerView.Model
{
  public struct NormalizedTimeRegisterValue : IEquatable<NormalizedTimeRegisterValue>, ISeries
  {
    public const string DummySerialNumber = "0";

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

    public NormalizedTimeRegisterValue Normalize(Func<DateTime, DateTime> timeDivider)
    {
      throw new NotSupportedException();
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[timeRegisterValue={0}, normalizedTimestamp={1}]", 
        timeRegisterValue, normalizedTimestamp.ToString("o"));
    }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != typeof(NormalizedTimeRegisterValue)) return false;
      var other = (NormalizedTimeRegisterValue)obj;
      return Equals(other);
    }

    public bool Equals(NormalizedTimeRegisterValue other)
    {
      return timeRegisterValue.Equals(other.timeRegisterValue) && normalizedTimestamp.Equals(other.normalizedTimestamp);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return timeRegisterValue.GetHashCode() ^ normalizedTimestamp.GetHashCode();
      }
    }

    public static bool operator ==(NormalizedTimeRegisterValue x, NormalizedTimeRegisterValue y) 
    {
      return x.Equals(y);
    }

    public static bool operator !=(NormalizedTimeRegisterValue x, NormalizedTimeRegisterValue y) 
    {
      return !(x == y);
    }

  }
}

