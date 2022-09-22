using System.Data;
using Dapper;

namespace PowerView.Model.Repository;

public static class DapperConfig
{
    private static bool configureComplete;

    public static void Configure()
    {
        if (configureComplete) return;

        SqlMapper.AddTypeHandler(new UnixTimeHandler());
        SqlMapper.AddTypeHandler(new DateTimeBlockerHandler());

        configureComplete = true;
    }

    /// <summary>
    /// Microsoft.Data.Sqlite defaults to string type in the database (and DateTimeKind.Unspecified).
    /// We want to use unix timestamps in the database to circumvent the (costy) string representations.
    /// This handler enables <see cref="UnixTime"/> for use with Dapper.
    /// </summary>
    internal class UnixTimeHandler : Dapper.SqlMapper.TypeHandler<UnixTime>
    {
        public override UnixTime Parse(object value)
        {
            if (value is long valueLong)
            {
                return new UnixTime(valueLong);
            }
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Type must be Int64. Was type:{value?.GetType().Name}");
        }

        public override void SetValue(IDbDataParameter parameter, UnixTime value)
        {
            parameter.Value = value.ToUnixTimeSeconds();
        }
    }


    /// <summary>
    /// Microsoft.Data.Sqlite defaults to string type in the database (and DateTimeKind.Unspecified).
    /// We want to use unix timestamps in the database to circumvent the (costy) string representations.
    /// This handler prevents use of <see cref="DateTime"/> with Dapper (more or less..)
    /// </summary>
    internal class DateTimeBlockerHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            throw new NotSupportedException("Do not use DateTime with Microsoft.Data.Sqlite as it enforces use of an underlying string. Use UnixTime instead, it uses Int64.");
            //            if (value.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(value), $"Must be UTC. Was:{value.Kind}");

            // Leave it to the provider to convert DateTime to string:
            // https://github.com/dotnet/efcore/blob/release/7.0/src/Microsoft.Data.Sqlite.Core/SqliteValueBinder.cs#L97
            //            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            throw new NotSupportedException("Do not use DateTime with Microsoft.Data.Sqlite as it enforces use of an underlying string. Use UnixTime instead, it uses Int64.");
        }
    }
}

