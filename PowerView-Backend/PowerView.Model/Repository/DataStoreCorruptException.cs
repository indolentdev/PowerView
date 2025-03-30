using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Repository
{
    [Serializable]
    public class DataStoreCorruptException : DataStoreException
    {
        public DataStoreCorruptException()
        {
        }

        public DataStoreCorruptException(string message)
          : base(message)
        {
        }

        public DataStoreCorruptException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}

