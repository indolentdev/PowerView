using System;
using System.Globalization;

namespace PowerView.Service.Mappers
{
  internal static class DateTimeMapper
  {
    public static string Map(DateTime timestamp)
    {
      if (timestamp.Kind != DateTimeKind.Utc)
      {
        throw new ArgumentOutOfRangeException("timestamp", "Must be UTC kind");
      }

      return timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    public static string Map(DateTime? timestamp)
    {
      if (timestamp == null) return null;

      return Map(timestamp.Value);
    }
  }
}

