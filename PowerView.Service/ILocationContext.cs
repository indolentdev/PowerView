using System;
using System.Globalization;

namespace PowerView.Service
{
  public interface ILocationContext
  {
    TimeZoneInfo TimeZoneInfo { get; }
    CultureInfo CultureInfo { get; }
  }
}
