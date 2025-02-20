using System;
namespace PowerView.Model
{
    public class CrudeDataValue
    {

        public CrudeDataValue(DateTime dateTime, ObisCode obisCode, int value, short scale, Unit unit, string deviceId, RegisterValueTag tag = RegisterValueTag.None)
        {
            if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");
            if (string.IsNullOrEmpty(deviceId)) throw new ArgumentNullException("deviceId");

            DateTime = dateTime;
            ObisCode = obisCode;
            Value = value;
            Scale = scale;
            Unit = unit;
            DeviceId = deviceId;
            Tag = tag;
        }

        public DateTime DateTime { get; private set; }
        public ObisCode ObisCode { get; private set; }
        public int Value { get; private set; }
        public short Scale { get; private set; }
        public Unit Unit { get; private set; }
        public string DeviceId { get; private set; }
        public RegisterValueTag Tag { get; private set; }
    }
}
