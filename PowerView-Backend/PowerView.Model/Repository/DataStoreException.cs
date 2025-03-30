using System;

namespace PowerView.Model.Repository
{
    public class DataStoreException : Exception
    {
        public DataStoreException()
        {
        }

        public DataStoreException(string message)
          : base(message)
        {
        }

        public DataStoreException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}

