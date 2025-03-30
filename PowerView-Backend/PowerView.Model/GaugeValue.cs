using System;
namespace PowerView.Model
{
  public class GaugeValue
  {

    public GaugeValue(string label, string deviceId, DateTime dateTime, ObisCode obisCode, UnitValue unitValue)
    {
      ArgCheck.ThrowIfNullOrEmpty(label);
      ArgCheck.ThrowIfNullOrEmpty(deviceId);
      ArgCheck.ThrowIfNotUtc(dateTime);

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
