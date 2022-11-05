
namespace PowerView.Model
{
    public class Reading
    {
        private readonly string label;
        private readonly string deviceId;
        private readonly DateTime timestamp;
        private readonly List<RegisterValue> registers;

        public Reading(string label, string deviceId, DateTime timestamp, IEnumerable<RegisterValue> registers)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
            if (string.IsNullOrEmpty(deviceId)) throw new ArgumentNullException("deviceId");
            if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("timestamp", "Must be UTC timestamp");
            if (registers == null || !registers.Any()) throw new ArgumentNullException("registers");

            this.label = label;
            this.deviceId = deviceId;
            this.timestamp = timestamp;
            this.registers = registers.ToList();
        }

        public string Label { get { return label; } }
        public string DeviceId { get { return deviceId; } }
        public DateTime Timestamp { get { return timestamp; } }

        public IReadOnlyList<RegisterValue> GetRegisterValues()
        {
            return registers;
        }
    }
}
