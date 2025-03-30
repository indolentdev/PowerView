namespace PowerView.Model;

public record MissingDate
{
    public DateTime Timestamp { get; private set; }
    public DateTime PreviousTimestamp { get; private set; }
    public DateTime NextTimestamp { get; private set; }

    public MissingDate(DateTime timestamp, DateTime previousTimestamp, DateTime nextTimestamp)
    {
        ArgCheck.ThrowIfNotUtc(timestamp);
        ArgCheck.ThrowIfNotUtc(previousTimestamp);
        ArgCheck.ThrowIfNotUtc(nextTimestamp);

        Timestamp = timestamp;
        PreviousTimestamp = previousTimestamp;
        NextTimestamp = nextTimestamp;
    }

}