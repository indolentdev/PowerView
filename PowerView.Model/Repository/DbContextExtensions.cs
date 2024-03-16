
namespace PowerView.Model.Repository;

internal static class DbContextExtensions
{
    public static IList<(byte Id, string Label)> GetLabelIds(this DbContext dbContext, IList<string> labels)
    {
        UnixTime now = DateTime.UtcNow;
        var labelsAndTimestamps = labels.Select(x => new { LabelName = x, Timestamp = now });
        dbContext.ExecuteTransaction(@"
              INSERT INTO Label (LabelName, Timestamp) VALUES (@LabelName, @Timestamp)
                ON CONFLICT(LabelName) DO UPDATE SET Timestamp = @Timestamp;", labelsAndTimestamps);

        return dbContext.QueryTransaction<(byte Id, string Label)>("SELECT Id, LabelName FROM Label;");
    }

    public static IList<(byte Id, string DeviceId)> GetDeviceIds(this DbContext dbContext, IList<string> deviceIds)
    {
        UnixTime now = DateTime.UtcNow;
        var deviceIdsAndTimestamps = deviceIds.Select(x => new { DeviceName = x, Timestamp = now });
        dbContext.ExecuteTransaction(@"
              INSERT INTO Device (DeviceName, Timestamp) VALUES (@DeviceName, @Timestamp)
                ON CONFLICT(DeviceName) DO UPDATE SET Timestamp = @Timestamp;", deviceIdsAndTimestamps);

        return dbContext.QueryTransaction<(byte Id, string DeviceId)>("SELECT Id, DeviceName FROM Device;");
    }

    public static IList<(byte Id, ObisCode ObisCode)> GetObisIds(this DbContext dbContext, IList<ObisCode> obisCodes)
    {
        var obisCodesLocal = obisCodes.Select(x => new { ObisCode = (long)x });
        dbContext.ExecuteTransaction(@"
              INSERT INTO Obis (ObisCode) VALUES (@ObisCode)
                ON CONFLICT(ObisCode) DO NOTHING;", obisCodesLocal);

        return dbContext.QueryTransaction<(byte Id, long ObisCode)>("SELECT Id, ObisCode FROM Obis;")
            .Select(x => (x.Id, (ObisCode)x.ObisCode))
            .ToList();
    }
}