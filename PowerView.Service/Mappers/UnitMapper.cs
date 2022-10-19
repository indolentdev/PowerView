using PowerView.Model;

namespace PowerView.Service.Mappers
{
    internal static class UnitMapper
    {
        public static string Map(Unit unit)
        {
            switch (unit)
            {
                case Unit.Watt:
                    return "W";
                case Unit.WattHour:
                    return "Wh";
                case Unit.CubicMetre:
                    return "m3";
                case Unit.CubicMetrePrHour:
                    return "m3/h";
                case Unit.DegreeCelsius:
                    return "C";
                case Unit.Joule:
                    return "J";
                case Unit.JoulePrHour:
                    return "J/h";
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
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            switch (unit)
            {
                case "Wh":
                    return Unit.WattHour;
                case "W":
                    return Unit.Watt;
                case "m3":
                    return Unit.CubicMetre;
                case "m3/h":
                    return Unit.CubicMetrePrHour;
                case "C":
                    return Unit.DegreeCelsius;
                case "J":
                    return Unit.Joule;
                case "J/h":
                    return Unit.JoulePrHour;
                case "%":
                    return Unit.Percentage;
                case "":
                    return Unit.NoUnit;

                default:
                    throw new ArgumentOutOfRangeException("unit", unit, "Unable to map unit:" + unit);
            }
        }
    }
}
