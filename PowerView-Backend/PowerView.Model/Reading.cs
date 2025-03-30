
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
            if (string.IsNullOrEmpty(label)) throw new ModelException("Label must be present");
            if (string.IsNullOrEmpty(deviceId)) throw new ModelException("DeviceId must be present");
            if (timestamp.Kind != DateTimeKind.Utc) throw new ModelException("Must be UTC timestamp");
            if (registers == null || !registers.Any()) throw new ModelException("At least one register must be present.");

            var registersLocal = registers.ToList();
            var duplicateObisCodes = registersLocal
              .GroupBy(x => x.ObisCode)
              .Select(x => new { ObisCode = x.Key, RegisterValues = x.ToList() })
              .Where(x => x.RegisterValues.Count > 1)
              .ToList();
            if (duplicateObisCodes.Count > 0)
            {
                var duplicateObisCodesString = string.Join(", ", duplicateObisCodes.Select(x => x.ObisCode));
                throw new ModelException($"Duplicate obis codes. Label:{label}, Timestamp:{timestamp.ToString("o")}, ObisCodes:{duplicateObisCodesString}");
            }

            this.label = label;
            this.deviceId = deviceId;
            this.timestamp = timestamp;
            this.registers = registersLocal;
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
