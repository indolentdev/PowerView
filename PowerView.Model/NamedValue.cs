using System;
namespace PowerView.Model
{
  public class NamedValue
  {
    public NamedValue(SerieName serieName, UnitValue unitValue)
    {
      if (serieName == null) throw new ArgumentNullException("serieName");

      SerieName = serieName;
      UnitValue = unitValue;
    }

    public SerieName SerieName { get; private set; }
    public UnitValue UnitValue { get; private set; }
  }
}
