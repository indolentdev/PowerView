using System.Data;
using System.Globalization;
using Dapper;

public static class DapperConfig
{
    private static bool configureComplete;

    public static void Configure()
    {
        if (configureComplete) return;

        SqlMapper.AddTypeHandler(new DateTimeUtcHandler());

        configureComplete = true;
    }

    /// <summary>
    /// Microsoft.Data.Sqlite defaults to string type in the database and DateTimeKind.Unspecified.
    /// PowerView uses invariant UTC. And the database historically uses unix timestamps (integer), from when "System.Data.SQLite" was used  on mono.
    /// So comply with that.
    /// </summary>
    private class DateTimeUtcHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value) 
        {
            if (value.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(value), $"Must be UTC. Was:{value.Kind}");
            parameter.Value = value.ToString("o", CultureInfo.InvariantCulture);
        }

        public override DateTime Parse(object value)
        {
            if (value is string stringValue)
            {
                var dateTime = DateTime.Parse(stringValue, CultureInfo.InvariantCulture);
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Must be string. Was:{value.GetType().Name}");
            }
        }
    }
}

