using System;
namespace PowerView.Model
{
  public class NamedValue
  {
    public NamedValue(SeriesName serieName, UnitValue unitValue)
    {
      ArgumentNullException.ThrowIfNull(serieName);

      SerieName = serieName;
      UnitValue = unitValue;
    }

    public SeriesName SerieName { get; private set; }
    public UnitValue UnitValue { get; private set; }
  }
}
