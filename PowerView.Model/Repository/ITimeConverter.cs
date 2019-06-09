using System;

namespace PowerView.Model.Repository
{
  public interface ITimeConverter
  {
    DateTime ChangeTimeZoneFromUtc(DateTime dateTime);

    DateTime ChangeTimeZoneToUtc(DateTime dateTime);

    DateTime Reduce(DateTime dateTime, DateTimeResolution resolution);

    bool IsGreaterThanResolutionFraction(DateTimeResolution resolution, double fraction, DateTime dateTime);
  }

  public enum DateTimeResolution
  {
    Day,
    Month,
    Year
  }
}

