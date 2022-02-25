using System;

namespace PowerView.Model
{
  public struct UnitValue : IEquatable<UnitValue>
  {
    private readonly double value;
    private readonly Unit unit;

    public double Value { get { return value; } }
    public Unit Unit { get { return unit; } }

    public UnitValue(double value, Unit unit)
    {
      this.value = value;
      this.unit = unit;
    }

    public UnitValue(int value, short scale, Unit unit)
    {
      this.value = value * Math.Pow(10, scale);
      this.unit = unit;
    }

    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[value={0}, unit={1}]", value, unit);
    }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != typeof(TimeRegisterValue)) return false;
      var other = (TimeRegisterValue)obj;
      return Equals(other);
    }

    public bool Equals(UnitValue other)
    {
      const double epsilon = 0.0000001;
      return Math.Abs(value - other.value) < epsilon && unit == other.unit;
    }

    public override int GetHashCode()
    {
      var decValue = (decimal)value;
      unchecked
      {
        int hash = 17;
        hash = hash * 23 + value.GetHashCode();
        hash = hash * 23 + unit.GetHashCode();
        return hash;
      }
    }

    public static bool operator ==(UnitValue x, UnitValue y) 
    {
      return x.Equals(y);
    }

    public static bool operator !=(UnitValue x, UnitValue y) 
    {
      return !(x == y);
    }

    public static UnitValue operator +(UnitValue a, UnitValue b)
    {
      AssertCompatibleUnits(a, b, "+");
      return new UnitValue(a.value + b.value, a.unit);
    }

    public static UnitValue operator -(UnitValue a, UnitValue b)
    {
      AssertCompatibleUnits(a, b, "-");
      return new UnitValue(a.value - b.value, a.unit);
    }

    private static void AssertCompatibleUnits(UnitValue a, UnitValue b, string operation)
    {
      if (a.Unit != b.Unit)
      {
        var msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, 
                    "A calculation was not possible due to incompatible units. Operation:{0}, Units:{1}, {2}", 
                    operation, a.Unit, b.Unit);
        throw new DataMisalignedException(msg);
      }
    }

  }
}

