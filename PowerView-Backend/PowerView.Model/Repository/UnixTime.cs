
namespace PowerView.Model.Repository;

internal struct UnixTime : IComparable<UnixTime>
{
    public static UnixTime Now => new UnixTime(DateTime.UtcNow);

    private readonly DateTime dateTime;

    public UnixTime(DateTime dateTime)
    {
        ArgCheck.ThrowIfNotUtc(dateTime);
        if (dateTime < DateTime.UnixEpoch) throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, "Must be equal to or after unix epoch");

        this.dateTime = dateTime;
    }

    public UnixTime(long unix) : this(DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime)
    {
    }

    public UnixTime() : this(DateTime.UnixEpoch)
    {
    }

    public long ToUnixTimeSeconds() => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

    public DateTime ToDateTime() => dateTime;

    public int CompareTo(UnixTime other)
    {
        return dateTime.CompareTo(other.dateTime);
    }

    public static implicit operator DateTime(UnixTime unixTime)
    {
        return unixTime.ToDateTime();
    }

    public static implicit operator UnixTime(DateTime dateTime)
    {
        return new UnixTime(dateTime);
    }
}
