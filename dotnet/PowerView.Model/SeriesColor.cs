using System;

namespace PowerView.Model
{
  public class SeriesColor
  {
    public SeriesColor(SeriesName seriesName, string color)
    {
      if (seriesName == null) throw new ArgumentNullException("seriesName");
      if (string.IsNullOrEmpty(color)) throw new ArgumentNullException("color");
      if (!IsColorValid(color)) throw new ArgumentOutOfRangeException("color", color, "Not a HTML color");

      SeriesName = seriesName;
      Color = color;
    }

    public SeriesName SeriesName { get; private set; }
    public string Color { get; private set; }

    public static bool IsColorValid(string color)
    {
      if (string.IsNullOrEmpty(color))
      {
        return false;
      }

      if (color.Length != 7 || color[0] != '#')
      {
        return false;
      }
      foreach (char c in color.Substring(1))
      {
        if (!char.IsDigit(c) && (c < 'a' && c > 'f') && (c < 'A' && c > 'F') )
        {
          return false;
        }
      }

      return true;
    }
  }
}

