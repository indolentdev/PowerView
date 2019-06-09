using System;
using System.Globalization;

namespace PowerView.Model.Repository
{
  public interface ILocationProvider
  {
    TimeZoneInfo GetTimeZone();

    CultureInfo GetCultureInfo();
  }
}

