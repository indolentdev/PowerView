using Microsoft.Data.Sqlite;

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
                // https://www3.sqlite.org/rescode.html
                if (e.SqliteErrorCode == 5 /*Busy*/) return new DataStoreBusyException(msg, e);
                if (e.SqliteErrorCode == 11 /*Corrupt*/) return new DataStoreCorruptException(msg, e);
                if (e.SqliteErrorCode == 19 /*Constraint*/ && e.SqliteExtendedErrorCode == 1555 /*PRIMARY KEY*/) return new DataStoreUniqueConstraintException(msg, e);
                if (e.SqliteErrorCode == 19 /*Constraint*/ && e.SqliteExtendedErrorCode == 2067 /*UNIQUE*/) return new DataStoreUniqueConstraintException(msg, e);
            }

            return new DataStoreException(msg, e);
        }
    }
}
