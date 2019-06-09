using System;

namespace PowerView.Model
{
  public class MeterEvent
  {
    public MeterEvent(string label, DateTime detectTimestamp, bool flag, IMeterEventAmplification amplification)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
      if (detectTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("detectTimestamp", "Must be UTC. Was:" + detectTimestamp.Kind);
      if (amplification == null) throw new ArgumentNullException("amplification");

      Label = label;
      DetectTimestamp = detectTimestamp;
      Flag = flag;
      Amplification = amplification;
    }

    public string Label { get; private set; }
    public DateTime DetectTimestamp { get; private set; }
    public bool Flag { get; private set; }
    public IMeterEventAmplification Amplification { get; private set; }
  }
}

