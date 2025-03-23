using System;

namespace PowerView.Model
{
  public struct DeviationValue : IEquatable<DeviationValue>
  {
    public DeviationValue(double value, double durationBasedDeviationMinValue, double durationBasedDeviationMaxValue)
    {
      Value = value;
      DurationBasedDeviationMinValue = durationBasedDeviationMinValue;
      DurationBasedDeviationMaxValue = durationBasedDeviationMaxValue;
    }

    public double Value { get; }

    public double DurationBasedDeviationMinValue { get; }

    public double DurationBasedDeviationMaxValue { get; }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[Value={0}, DurationBasedDeviationMinValue={1}, DurationBasedDeviationMaxValue={2}]", Value, DurationBasedDeviationMinValue, DurationBasedDeviationMaxValue);
    }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != typeof(DeviationValue)) return false;
      var other = (DeviationValue)obj;
      return Equals(other);
    }

    public bool Equals(DeviationValue other)
    {
      const double epsilon = 0.0000001;
      return Math.Abs(Value - other.Value) < epsilon && Math.Abs(DurationBasedDeviationMinValue - other.DurationBasedDeviationMinValue) < epsilon && Math.Abs(DurationBasedDeviationMaxValue - other.DurationBasedDeviationMaxValue) < epsilon;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 23 + Value.GetHashCode();
        hash = hash * 23 + DurationBasedDeviationMinValue.GetHashCode();
        hash = hash * 23 + DurationBasedDeviationMaxValue.GetHashCode();
        return hash;
      }
    }

    public static bool operator ==(DeviationValue x, DeviationValue y) 
    {
      return x.Equals(y);
    }

    public static bool operator !=(DeviationValue x, DeviationValue y) 
    {
      return !(x == y);
    }

  }
}

