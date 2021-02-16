using System;
using System.Collections.Generic;

namespace PowerView.Model
{
  public class PeriodRegisterValue : IEquatable<PeriodRegisterValue>
  {
    private readonly DateTime startTimestamp;
    private readonly DateTime endTimestamp;
    private readonly UnitValue unitValue;

    public DateTime StartTimestamp { get { return startTimestamp; } }
    public DateTime EndTimestamp { get { return endTimestamp; } }
    public UnitValue UnitValue { get { return unitValue; } }

    public PeriodRegisterValue(DateTime startTimestamp, DateTime endTimestamp, UnitValue unitValue)
    {
      if (startTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("startTimestamp", "Must be UTC");
      if (endTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("endTimestamp", "Must be UTC");
      if (startTimestamp > endTimestamp) throw new ArgumentOutOfRangeException("endTimetamp", "Must be after startTimetamp");

      this.startTimestamp = startTimestamp;
      this.endTimestamp = endTimestamp;
      this.unitValue = unitValue;
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[startTimestamp={0}, endTimestamp={1}, unitValue={2}]", 
        startTimestamp.ToString("o"), endTimestamp.ToString("o"), unitValue);
    }

    public override bool Equals(object obj)
    {
      var value = obj as PeriodRegisterValue;
      return Equals(value);
    }

    public bool Equals(PeriodRegisterValue value)
    {
      return value != null &&
             startTimestamp == value.startTimestamp &&
             endTimestamp == value.endTimestamp &&
             unitValue.Equals(value.unitValue);
    }

    public override int GetHashCode()
    {
      var hashCode = -1204581493;
      hashCode = hashCode * -1521134295 + startTimestamp.GetHashCode();
      hashCode = hashCode * -1521134295 + endTimestamp.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<UnitValue>.Default.GetHashCode(unitValue);
      return hashCode;
    }

    public static bool operator ==(PeriodRegisterValue value1, PeriodRegisterValue value2)
    {
      return EqualityComparer<PeriodRegisterValue>.Default.Equals(value1, value2);
    }

    public static bool operator !=(PeriodRegisterValue value1, PeriodRegisterValue value2)
    {
      return !(value1 == value2);
    }
  }
}
