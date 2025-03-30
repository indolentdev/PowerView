using System;
using System.Globalization;

namespace PowerView.Model.Repository
{
    /// <summary>
    /// Gets the appropriate location/region info.
    /// Intended for application init.
    /// </summary>
    public interface ILocationProvider
    {
        TimeZoneInfo GetTimeZone();

        CultureInfo GetCultureInfo();
    }
}

