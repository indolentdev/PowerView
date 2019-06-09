using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class LiveReading
  {
    private readonly string label;
    private readonly string serialNumber;
    private readonly DateTime timestamp;
    private readonly RegisterValue[] registers;

    public LiveReading(string label, string serialNumber, DateTime timestamp, IEnumerable<RegisterValue> registers)
    {
      if ( string.IsNullOrEmpty(label) ) throw new ArgumentNullException("label");
      if ( string.IsNullOrEmpty(serialNumber)) throw new ArgumentNullException("serialNumber");
      if ( timestamp.Kind != DateTimeKind.Utc ) throw new ArgumentOutOfRangeException("timestamp", "Must be UTC timestamp");
      if ( registers == null || !registers.Any() ) throw new ArgumentNullException("registers");
      if (registers.Any(r => r == null)) throw new ArgumentOutOfRangeException("registers", "Must not contain nulls");

      this.label = label;
      this.serialNumber = serialNumber;
      this.timestamp = timestamp;
      this.registers = registers.ToArray();
    }

    public string Label { get { return label; } }
    public string SerialNumber { get { return serialNumber; } }
    public DateTime Timestamp { get { return timestamp; } }

    public RegisterValue[] GetRegisterValues()
    {
      return registers.ToArray();
    }
  }
}
