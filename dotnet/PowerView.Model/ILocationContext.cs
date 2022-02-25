using System;
using System.Globalization;

namespace PowerView.Model
{
  /// <summary>
  /// Location/region info as a cheap dependency.
  /// </summary>
  public interface ILocationContext
  {
    TimeZoneInfo TimeZoneInfo { get; }
    CultureInfo CultureInfo { get; }

    DateTime ConvertTimeFromUtc(DateTime dateTime);
    DateTime ConvertTimeToUtc(DateTime dateTime);
  }
}
