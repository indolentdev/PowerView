namespace PowerView.Model;

public record MissingDate
{
    public DateTime Timestamp { get; private set; }
    public DateTime PreviousTimestamp { get; private set; }
    public DateTime NextTimestamp { get; private set; }

    public MissingDate(DateTime timestamp, DateTime previousTimestamp, DateTime nextTimestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(timestamp), $"Must be UTC. Was:{timestamp.Kind}");
        if (previousTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(previousTimestamp), $"Must be UTC. Was:{previousTimestamp.Kind}");
        if (nextTimestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(nextTimestamp), $"Must be UTC. Was:{nextTimestamp.Kind}");

        Timestamp = timestamp;
        PreviousTimestamp = previousTimestamp;
        NextTimestamp = nextTimestamp;
    }

}