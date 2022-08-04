using System.Data.SQLite;

namespace PowerView.Model.Repository
{
  public static class DataStoreExceptionFactory
  {
    private const string defaultMessage = "Database operation failed";
    
    public static DataStoreException Create(SQLiteException e, string message = null)
    {
      var msg = message ?? defaultMessage;

      if (e != null)
      {
        if (e.ResultCode == SQLiteErrorCode.Busy) return new DataStoreBusyException(msg, e);
        if (e.ResultCode == SQLiteErrorCode.Corrupt) return new DataStoreCorruptException(msg, e);
        if (e.ResultCode == SQLiteErrorCode.Constraint && e.Message.Contains("UNIQUE")) return new DataStoreUniqueConstraintException(msg, e);
      }
      
      return new DataStoreException(msg, e);
    }
  }
}

