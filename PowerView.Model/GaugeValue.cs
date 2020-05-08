using System;
namespace PowerView.Model
{
  public class GaugeValue
  {

    public GaugeValue(string label, string deviceId, DateTime dateTime, ObisCode obisCode, UnitValue unitValue)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
      if (string.IsNullOrEmpty(deviceId)) throw new ArgumentNullException("deviceId");
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");

      Label = label;
      DeviceId = deviceId;
      DateTime = dateTime;
      ObisCode = obisCode;
      UnitValue = unitValue;
    }

    public string Label { get; private set; }
    public string DeviceId { get; private set; }
    public DateTime DateTime { get; private set; }
    public ObisCode ObisCode { get; private set; }
    public UnitValue UnitValue { get; private set; }
  }
}
