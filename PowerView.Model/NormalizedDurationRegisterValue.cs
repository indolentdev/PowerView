using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class NormalizedDurationRegisterValue : IEquatable<NormalizedDurationRegisterValue>, IRegisterValue
  {
    private readonly DateTime start;
    private readonly DateTime end;
    private readonly DateTime normalizedStart;
    private readonly DateTime normalizedEnd;
    private readonly UnitValue unitValue;
    private readonly IReadOnlyList<string> deviceIds;

    public DateTime Start { get { return start; } }
    public DateTime End { get { return end; } }
    public DateTime NormalizedStart { get { return normalizedStart; } }
    public DateTime NormalizedEnd { get { return normalizedEnd; } }
    public UnitValue UnitValue { get { return unitValue; } }
    public IReadOnlyList<string> DeviceIds { get { return deviceIds; } }

    public DateTime OrderProperty { get { return End; } }

    public TimeSpan Duration => End - Start;
    public TimeSpan NormalizedDuration => NormalizedEnd - NormalizedStart;

    public NormalizedDurationRegisterValue(DateTime start, DateTime end, DateTime normalizedStart, DateTime normalizedEnd, UnitValue unitValue, params string[] deviceIds)
      : this(start, end, normalizedStart, normalizedEnd, unitValue, (IEnumerable<string>)deviceIds)
    {

    }

    public NormalizedDurationRegisterValue(DateTime start, DateTime end, DateTime normalizedStart, DateTime normalizedEnd, UnitValue unitValue, IEnumerable<string> deviceIds)
    {
      if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("start", "Must be UTC");
      if (end.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("end", "Must be UTC");
      if (start > end) throw new ArgumentOutOfRangeException("end", "Must be after startTimetamp");
      if (normalizedStart.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("normalizedStart", "Must be UTC");
      if (normalizedEnd.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("normalizedEnd", "Must be UTC");
      if (normalizedStart > normalizedEnd) throw new ArgumentOutOfRangeException("normalizedEnd", "Must be after startTimetamp");

      this.start = start;
      this.end = end;
      this.normalizedStart = normalizedStart;
      this.normalizedEnd = normalizedEnd;
      this.unitValue = unitValue;
      this.deviceIds = DeviceId.DistinctDeviceIds(deviceIds);
    }

    public double GetDurationDeviationRatio()
    {
      var deviation = Duration - NormalizedDuration;
      if (deviation < TimeSpan.Zero) deviation *= -1; // absolute deviation

      return deviation / NormalizedDuration;
    }

    public double GetPointDeviationRatio()
    {
      var startDeviation = Start - NormalizedStart;
      if (startDeviation < TimeSpan.Zero) startDeviation *= -1; // absolute deviation

      var endDeviation = End - NormalizedEnd;
      if (endDeviation < TimeSpan.Zero) endDeviation *= -1; // absolute deviation

      var deviation = startDeviation + endDeviation;

      return deviation / NormalizedDuration;
    }

    public NormalizedDurationRegisterValue SubtractNotNegative(NormalizedDurationRegisterValue baseValue)
    {
      var substractedValue = UnitValue - baseValue.UnitValue;
      var dValue = substractedValue.Value;

      if (dValue < 0)
      {
        dValue = 0;
      }

      var newStart = new DateTime(Math.Min(Start.Ticks, baseValue.Start.Ticks), DateTimeKind.Utc);
      var newEnd = new DateTime(Math.Max(End.Ticks, baseValue.End.Ticks), DateTimeKind.Utc);
      var newNormStart = new DateTime(Math.Min(NormalizedStart.Ticks, baseValue.NormalizedStart.Ticks), DateTimeKind.Utc);
      var newNormEnd = new DateTime(Math.Max(NormalizedEnd.Ticks, baseValue.NormalizedEnd.Ticks), DateTimeKind.Utc);
      var newValue = new NormalizedDurationRegisterValue(newStart, newEnd, newNormStart, newNormEnd,
        new UnitValue(dValue, substractedValue.Unit), DeviceIds.Concat(baseValue.DeviceIds));

      return newValue;
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[start={0}, end={1}, normalizedStart={2}, normalizedEnd={3}, unitValue={4}, deviceIds=[{5}]]", 
        start.ToString("o"), end.ToString("o"), normalizedStart.ToString("o"), normalizedEnd.ToString("o"), unitValue, string.Join(", ", deviceIds));
    }

    public override bool Equals(object obj)
    {
      var value = obj as NormalizedDurationRegisterValue;
      return Equals(value);
    }

    public bool Equals(NormalizedDurationRegisterValue value)
    {
      return value != null &&
             start == value.start &&
             end == value.end &&
             normalizedStart == value.normalizedStart &&
             normalizedEnd == value.normalizedEnd &&
             unitValue.Equals(value.unitValue) &&
             deviceIds.SequenceEqual(value.deviceIds);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = 1655761989;
        hashCode = hashCode * -1521134295 + start.GetHashCode();
        hashCode = hashCode * -1521134295 + end.GetHashCode();
        hashCode = hashCode * -1521134295 + normalizedStart.GetHashCode();
        hashCode = hashCode * -1521134295 + normalizedEnd.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<UnitValue>.Default.GetHashCode(unitValue);
        hashCode = hashCode * -1521134295 + HashCodeSum(deviceIds);
        return hashCode;
      }
    }

    private static int HashCodeSum(IEnumerable<string> strings)
    {
      unchecked
      {
        int hashCodeSum = 0;
        foreach (var s in strings)
        {
          hashCodeSum += s.GetHashCode();
        }
        return hashCodeSum;
      }
    }

    public static bool operator ==(NormalizedDurationRegisterValue value1, NormalizedDurationRegisterValue value2)
    {
      return EqualityComparer<NormalizedDurationRegisterValue>.Default.Equals(value1, value2);
    }

    public static bool operator !=(NormalizedDurationRegisterValue value1, NormalizedDurationRegisterValue value2)
    {
      return !(value1 == value2);
    }
  }
}
