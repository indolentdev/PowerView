
namespace PowerView.Model
{
    public enum Unit : byte
    {
        /// <summary>
        /// Power: Watt (Wh)
        /// </summary>
        Watt = 0,
        /// <summary>
        /// Energy: WattHour (Wh)
        /// </summary>
        WattHour = 1,
        /// <summary>
        /// Volume: CubicMetre (m3)
        /// </summary>
        CubicMetre = 2,
        /// <summary>
        /// Volume per hour: CubicMetre per hour (m3/h)
        /// </summary>
        CubicMetrePrHour = 3,
        /// <summary>
        /// Temperature: Degree Celsius (C)
        /// </summary>
        DegreeCelsius = 4,
        /// <summary>
        /// Energy: Joule (J)
        /// </summary>
        Joule = 5,
        /// <summary>
        /// Power: Joule per hour (J/h)
        /// </summary>
        JoulePrHour = 6,
        /// <summary>
        /// Percentage: %
        /// </summary>
        Percentage = 7,

        /// <summary>
        /// Currency: European Euros (EUR)
        /// </summary>
        Eur = 110,

        /// <summary>
        /// Currency: Danish Crowns (DKK)
        /// </summary>
        Dkk = 111,


        /// <summary>
        /// No unit
        /// </summary>
        NoUnit = 120
    }
}

