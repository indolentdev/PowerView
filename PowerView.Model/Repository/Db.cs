using System;

namespace PowerView.Model.Repository
{
    internal interface IDbEntity
    {
        long Id { get; set; }
    }

    internal interface IDbReading : IDbEntity
    {
        byte LabelId { get; set; }
        byte DeviceId { get; set; }
        UnixTime Timestamp { get; set; }
    }

    internal interface IDbRegister
    {
        long ReadingId { get; set; }
        byte ObisId { get; set; }
        int Value { get; set; }
        short Scale { get; set; }
        byte Unit { get; set; }
    }

    internal interface IDbRegisterTag
    {
        public long ReadingId { get; set; }
        public byte ObisId { get; set; }
        public byte Tags { get; set; }
    }

    internal static class Db
    {
        public class LiveReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public UnixTime Timestamp { get; set; }
        }

        public class LiveRegister : IDbRegister
        {
            public long ReadingId { get; set; }
            public byte ObisId { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }

        public class LiveRegisterTag : IDbRegisterTag
        {
            public long ReadingId { get; set; }
            public byte ObisId { get; set; }
            public byte Tags { get; set; }
        }

        public class DayReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public UnixTime Timestamp { get; set; }
        }

        public class DayRegister : IDbRegister
        {
            public long ReadingId { get; set; }
            public byte ObisId { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }

        public class MonthReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public UnixTime Timestamp { get; set; }
        }

        public class MonthRegister : IDbRegister
        {
            public long ReadingId { get; set; }
            public byte ObisId { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }

        public class YearReading : IDbReading
        {
            public long Id { get; set; }
            public byte LabelId { get; set; }
            public byte DeviceId { get; set; }
            public UnixTime Timestamp { get; set; }
        }

        public class YearRegister : IDbRegister
        {
            public long ReadingId { get; set; }
            public byte ObisId { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
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
            public UnixTime DetectTimestamp { get; set; }
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

        public class CostBreakdown : IDbEntity
        {
            public long Id { get; set; }
            public string Title { get; set; }
            public int Currency { get; set; }
            public int Vat { get; set; }
        }

        public class CostBreakdownEntry
        {
            public long CostBreakdownId { get; set; }
            public UnixTime FromDate { get; set; }
            public UnixTime ToDate { get; set; }
            public string Name { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public double Amount { get; set; }
        }

        public class Import : IDbEntity
        {
            public long Id { get; set; }
            public string Label { get; set; }
            public string Channel { get; set; }
            public int Currency { get; set; }
            public UnixTime FromTimestamp { get; set; }
            public UnixTime? CurrentTimestamp { get; set; }
            public bool Enabled { get; set; }
        }

        public class GeneratorSeries
        {
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public byte BaseLabelId { get; set; }
            public byte BaseObisId { get; set; }
            public string CostBreakdownTitle { get; set; }
        }

        public class GeneratorSeriesEx
        {
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public string BaseLabel { get; set; }
            public long BaseObisCode { get; set; }
            public string CostBreakdownTitle { get; set; }
        }

        public class LabelObisLive
        {
            public byte LabelId { get; set; }
            public byte ObisId { get; set; }
            public UnixTime LatestTimestamp { get; set; }
        }

    }
}
