using System;

namespace PowerView.Model
{
  public class SerieColor
  {
    public SerieColor(string label, ObisCode obisCode, string color)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
      if (string.IsNullOrEmpty(color)) throw new ArgumentNullException("color");
      if (!IsColorValid(color)) throw new ArgumentOutOfRangeException("color", color, "Not a HTML color");

      Label = label;
      ObisCode = obisCode;
      Color = color;
    }

    public string Label { get; private set; }
    public ObisCode ObisCode { get; private set; }
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

