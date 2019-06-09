using Mono.Data.Sqlite;

namespace PowerView.Model.Repository
{
  public static class DataStoreExceptionFactory
  {
    private const string defaultMessage = "Database operation failed";
    
    public static DataStoreException Create(SqliteException e, string message = null)
    {
      var msg = message ?? defaultMessage;

      if (e != null)
      {
        if (e.ErrorCode == SQLiteErrorCode.Busy) return new DataStoreBusyException(msg, e);
        if (e.ErrorCode == SQLiteErrorCode.Corrupt) return new DataStoreCorruptException(msg, e);
        if (e.ErrorCode == SQLiteErrorCode.Constraint && e.Message.Contains("UNIQUE")) return new DataStoreUniqueConstraintException(msg, e);
      }
      
      return new DataStoreException(msg, e);
    }
  }
}

