using System;
namespace PowerView.Model
{
  public class GaugeValue
  {

    public GaugeValue(string label, string serialNumber, DateTime dateTime, ObisCode obisCode, UnitValue unitValue)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
      if (string.IsNullOrEmpty(serialNumber)) throw new ArgumentNullException("serialNumber");
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");

      Label = label;
      SerialNumber = serialNumber;
      DateTime = dateTime;
      ObisCode = obisCode;
      UnitValue = unitValue;
    }

    public string Label { get; private set; }
    public string SerialNumber { get; private set; }
    public DateTime DateTime { get; private set; }
    public ObisCode ObisCode { get; private set; }
    public UnitValue UnitValue { get; private set; }
  }
}
