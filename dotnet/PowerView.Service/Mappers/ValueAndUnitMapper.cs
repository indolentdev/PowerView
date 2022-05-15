using System;
using PowerView.Model;

namespace PowerView.Service.Mappers
{
  internal static class ValueAndUnitMapper
  {
    private static readonly double jouleToKiloWattHourFactor = 2.7777777778 * Math.Pow(10, -7);
    private static readonly double joulePrHourToWattFactor = 0.00027777777778;

    public static double? Map(double? value, Unit unit, bool reduceUnit = false)
    {
      if (value == null)
      {
        return null;
      }

      double newValue = 0;
      switch (unit)
      {
        case Unit.WattHour:
          newValue = value.Value / 1000;
          break;
        case Unit.Joule:
          newValue = value.Value * jouleToKiloWattHourFactor;
          break;
        case Unit.JoulePrHour:
          newValue = value.Value * joulePrHourToWattFactor;  
          break;
        case Unit.CubicMetrePrHour:
          newValue = value.Value * 1000;  
          break;
        case Unit.CubicMetre:
          if (reduceUnit) newValue = value.Value * 1000; else newValue = value.Value;
          break;
        default:
          newValue = value.Value;
          break;
      }

      return Math.Round(newValue, 3);
    }

    public static string Map(Unit unit, bool reduceUnit = false)
    {
      switch (unit)
      {
        case Unit.WattHour:
        case Unit.Joule:
          return "kWh";
        case Unit.Watt:
        case Unit.JoulePrHour:
          return "W";
        case Unit.DegreeCelsius:
          return "C";
        case Unit.CubicMetre:
          if (reduceUnit) return "l"; else return "m3";
        case Unit.CubicMetrePrHour:
          return "l/h";
        case Unit.Percentage:
          return "%";
        case Unit.NoUnit:
          return string.Empty;
        default:
          return "Unknown";
      }
    }

    public static Unit Map(string unit)
    {
      if (string.IsNullOrEmpty(unit)) throw new ArgumentNullException("unit");
        
      switch (unit)
      {
        case "W":
          return Unit.Watt;
        default:
          throw new ArgumentOutOfRangeException("unit", unit, "Unable to map unit:" + unit);
      }
    }

  }
}
