using System;

namespace PowerView.Model.Repository
{
    public interface IDbEntity
    {
        long Id { get; set; }
    }

    public interface IDbReading : IDbEntity
    {
        byte LabelId { get; set; }
        byte DeviceId { get; set; }
        DateTime Timestamp { get; set; }
    }

    public interface IDbRegister
    {
        long ObisCode { get; set; }
        int Value { get; set; }
        short Scale { get; set; }
        byte Unit { get; set; }
        long ReadingId { get; set; }
    }

    internal static class Db
    {
        public class LiveReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class LiveRegister : IDbRegister
        {
            public long ReadingId { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }

        public class DayReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class DayRegister : IDbRegister
        {
            public long Id { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
            public long ReadingId { get; set; }
        }

        public class MonthReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class MonthRegister : IDbRegister
        {
            public long Id { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
            public long ReadingId { get; set; }
        }

        public class YearReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class YearRegister : IDbRegister
        {
            public long Id { get; set; }
            public long ObisCode { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
            public long ReadingId { get; set; }
        }

        public class StreamPosition : IDbEntity
        {
            public long Id { get; set; }
            public string StreamName { get; set; }
            public byte LabelId { get; set; }
            public long Position { get; set; }
        }

        public class Setting : IDbEntity
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class SerieColor : IDbEntity
        {
            public long Id { get; set; }
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public string Color { get; set; }
        }

        public class ProfileGraph : IDbEntity
        {
            public long Id { get; set; }
            public string Period { get; set; }
            public string Page { get; set; }
            public string Title { get; set; }
            public string Interval { get; set; }
            public long Rank { get; set; }
        }

        public class ProfileGraphSerie : IDbEntity
        {
            public long Id { get; set; }
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public long ProfileGraphId { get; set; }
        }

        public class EmailRecipient : IDbEntity
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string EmailAddress { get; set; }
        }

        public class EmailRecipientMeterEventPosition : IDbEntity
        {
            public long Id { get; set; }
            public long EmailRecipientId { get; set; }
            public long MeterEventId { get; set; }
        }

        public class EmailMessage : IDbEntity
        {
            public long Id { get; set; }
            public string FromName { get; set; }
            public string FromEmailAddress { get; set; }
            public string ToName { get; set; }
            public string ToEmailAddress { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public class MeterEvent : IDbEntity
        {
            public long Id { get; set; }
            public string Label { get; set; }
            public string MeterEventType { get; set; }
            public DateTime DetectTimestamp { get; set; }
            public bool Flag { get; set; }
            public string Amplification { get; set; }
        }

        public class DisconnectRule : IDbEntity
        {
            public long Id { get; set; }
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public string EvaluationLabel { get; set; }
            public long EvaluationObisCode { get; set; }
            public int DurationSeconds { get; set; }
            public int DisconnectToConnectValue { get; set; }
            public int ConnectToDisconnectValue { get; set; }
            public byte Unit { get; set; }
        }

    }
}
