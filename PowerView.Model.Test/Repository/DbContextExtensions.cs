using System.Globalization;
using System.Linq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  internal static class DbContextExtensions
  {
    internal static int GetCount<TEntity>(this DbContext dbContext)
    {
      var table = typeof(TEntity).Name;
      var sql = "SELECT count(*) FROM {0};";
      sql = string.Format(CultureInfo.InvariantCulture, sql, table);
      return dbContext.QueryTransaction<int>("", sql).First();
    }

    internal static void InsertReadings(this DbContext dbContext, params IDbReading[] dbReadings)
    {
      if (dbReadings.Length == 0) return;

      var sql = "INSERT INTO {0} (Label,DeviceId,Timestamp) VALUES (@Label,@DeviceId,@Timestamp); SELECT last_insert_rowid();";
      foreach (var dbReading in dbReadings)
      {
        var tableName = dbReading.GetType().Name;
        var readingSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
        var id = dbContext.QueryTransaction<long>("", readingSql, dbReading).First();
        dbReading.Id = id;
      }
    }

    internal static void InsertRegisters(this DbContext dbContext, params IDbRegister[] dbRegisters)
    {
      if (dbRegisters.Length == 0) return;

      var sql = "INSERT INTO {0} (ObisCode,Value,Scale,Unit,ReadingId) VALUES (@ObisCode,@Value,@Scale,@Unit,@ReadingId);";

      var dbRegisterGroups = dbRegisters.GroupBy(x => x.GetType().Name);
      foreach (var group in dbRegisterGroups)
      {
        var tableName = group.Key;
        var groupSql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
        dbContext.ExecuteTransaction("", groupSql, group);
      }
    }

  }
}
