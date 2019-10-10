using System;
using System.Globalization;

namespace PowerView.Service
{
  public interface IApplicationConfiguraiton
  {
    TimeZoneInfo TimeZoneInfo { get; }
    CultureInfo CultureInfo { get; }
  }
}
