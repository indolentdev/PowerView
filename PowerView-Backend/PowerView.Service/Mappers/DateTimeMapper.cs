using System;
using System.Globalization;
using PowerView.Model;

namespace PowerView.Service.Mappers
{
  internal static class DateTimeMapper
  {
    public static string Map(DateTime timestamp)
    {
      ArgCheck.ThrowIfNotUtc(timestamp);

      return timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    public static string Map(DateTime? timestamp)
    {
      if (timestamp == null) return null;

      return Map(timestamp.Value);
    }
  }
}

